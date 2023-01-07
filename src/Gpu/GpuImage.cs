using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using OpenCL.Net;
using PokemonSolver.Memory;
using Environment = System.Environment;
using ImageFormat = OpenCL.Net.ImageFormat;

namespace PokemonSolver.Gpu
{
    public abstract class GpuImage : GpuHandler
    {
        protected readonly ImageFormat ImageFormat;

        protected int ImageWidth;

        protected int ImageHeight;
        //TODO

        protected GpuImage(string functionName) : base(functionName)
        {
            ImageFormat = new ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);
            ImageWidth = ImageHeight = -1;
        }

        protected override void AllocateMemory()
        {
            throw new System.NotImplementedException();
        }

        public void GetImage() //TODO return bitmap
        {
            if (ImageWidth == -1 || ImageHeight == -1)
                throw new Exception("Trying to compute uninitialized image");

            // var imageStride = 4;
            Event clEvent;
            byte[] outputByteArray = new byte[ImageWidth * ImageHeight * 4];

            IMem outputImage2DBuffer = Cl.CreateImage2D(
                GpuContext,
                MemFlags.CopyHostPtr | MemFlags.WriteOnly,
                ImageFormat,
                (IntPtr)ImageWidth,
                (IntPtr)ImageHeight,
                (IntPtr)0,
                outputByteArray,
                out ErrorCode
            );

            // ErrorCode = AddArg()
            // ErrorCode = AddArg(IntPtrSize, inputImage2DBuffer);
            ErrorCode = AddArg(IntPtrSize, outputImage2DBuffer);
            // ErrorCode = Cl.SetKernelArg(Kernel, 0, (IntPtr)IntPtrSize, inputImage2DBuffer);
            // ErrorCode |= Cl.SetKernelArg(Kernel, 1, (IntPtr)IntPtrSize, outputImage2DBuffer);
            CheckError("Cl.SetKernelArg");

            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 }; //x, y, z
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)ImageWidth, (IntPtr)ImageHeight, (IntPtr)1 }; //x, y, z
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)ImageWidth, (IntPtr)ImageHeight, (IntPtr)1 };

            ErrorCode = Cl.EnqueueNDRangeKernel(
                CommandQueue,
                Kernel,
                2,
                null,
                workGroupSizePtr,
                null,
                0,
                null,
                out clEvent
            );
            CheckError("Cl.EnqueueNDRangeKernel");

            Utils.Log("enqueue nd range OK");
            ErrorCode = Cl.Finish(CommandQueue);
            CheckError("Cl.Finish");
            Utils.Log("finish OK");
            ErrorCode = Cl.EnqueueReadImage(
                CommandQueue,
                outputImage2DBuffer,
                Bool.True,
                originPtr,
                regionPtr,
                (IntPtr)0,
                (IntPtr)0,
                outputByteArray,
                0,
                null,
                out clEvent
            );
            CheckError("Cl.clEnqueueReadImage");

            Utils.Log("enqueue read image OK");
            Cl.ReleaseKernel(Kernel);
            Cl.ReleaseCommandQueue(CommandQueue);

            Cl.ReleaseMemObject(outputImage2DBuffer);


            GCHandle pinnedOutputArray = GCHandle.Alloc(outputByteArray, GCHandleType.Pinned);
            IntPtr outputBmpPointer = pinnedOutputArray.AddrOfPinnedObject();
            //Create a new bitmap with processed data and save it to a file.

            var format = PixelFormat.Format32bppArgb;
            
            Bitmap outputBitmap = new Bitmap(
                ImageWidth,
                ImageHeight,
                ImageWidth * 4,
                format,
                outputBmpPointer);

            var outputPath = $"{Environment.CurrentDirectory}/../Images/pouet-post.png";
            Utils.Log($"Trying to save image to {outputPath}");
            outputBitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
            pinnedOutputArray.Free();

            // return new Bitmap(ImageWidth, ImageHeight); //TODO
            //
            // return new Bitmap(
            //     ImageWidth,
            //     ImageHeight,
            //     ImageWidth, //maybe ?
            //     PixelFormat.Format32bppArgb,
            //     outputBmpPointer
            // );
        }
    }
}