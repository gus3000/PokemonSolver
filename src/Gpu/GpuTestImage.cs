using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenCL.Net;
using PokemonSolver.Memory;
using Environment = System.Environment;
using ImageFormat = OpenCL.Net.ImageFormat;

namespace PokemonSolver.Gpu
{
    public class GpuTestImage
    {
        private Context _context;
        private Device _device;

        public GpuTestImage()
        {
            Setup();
        }

        private void CheckError(ErrorCode err, string name)
        {
            if (err != ErrorCode.Success)
            {
                throw new Exception($"ERROR: {name} ({err})");
            }
        }

        private void ContextNotify(string errInfo, byte[] data, IntPtr cb, IntPtr userData)
        {
            Utils.Log($"OpenCL Notification: {errInfo}");
        }

        private void Setup()
        {
            ErrorCode error;
            Platform[] platforms = Cl.GetPlatformIDs(out error);
            List<Device> devicesList = new List<Device>();

            CheckError(error, "GetPlatformIDs");

            foreach (Platform platform in platforms)
            {
                string platformName = Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error).ToString();
                Utils.Log("Platform: " + platformName);
                CheckError(error, "GetPlatformInfo");
                //We will be looking only for GPU devices
                foreach (Device device in Cl.GetDeviceIDs(platform, DeviceType.Gpu, out error))
                {
                    CheckError(error, "GetDeviceIDs");
                    Utils.Log("Device: " + device.ToString());
                    devicesList.Add(device);
                }
            }

            if (devicesList.Count <= 0)
            {
                Utils.Log("No devices found.");
                return;
            }

            _device = devicesList[0];

            if (Cl.GetDeviceInfo(_device, DeviceInfo.ImageSupport,
                    out error).CastTo<Bool>() == Bool.False)
            {
                Utils.Log("No image support.");
                return;
            }

            _context
                = Cl.CreateContext(null, 1, new[] { _device }, ContextNotify,
                    IntPtr.Zero, out error); //Second parameter is amount of devices
            CheckError(error, "CreateContext");
        }

        public void ImagingTest(string inputImagePath, string outputImagePath)
        {
            ErrorCode error;
            //Load and compile kernel source code.
            // string programPath = Environment.CurrentDirectory + "/../../ImagingTest.cl";
            string programPath = $"{Environment.CurrentDirectory}\\..\\src\\Gpu\\HLSL\\ImagingTest.cl";
            //The path to the source file may vary

            if (!System.IO.File.Exists(programPath))
            {
                Utils.Log("Program doesn't exist at path " + programPath);
                return;
            }

            string programSource = System.IO.File.ReadAllText(programPath);

            using (Program program = Cl.CreateProgramWithSource(_context, 1, new[] { programSource }, null, out error))
            {
                CheckError(error, "Cl.CreateProgramWithSource");
                //Compile kernel source
                error = Cl.BuildProgram(program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
                CheckError(error, "Cl.BuildProgram");
                //Check for any compilation errors
                if (Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Status, out error).CastTo<BuildStatus>()
                    != BuildStatus.Success)
                {
                    CheckError(error, "Cl.GetProgramBuildInfo");
                    Utils.Log("Cl.GetProgramBuildInfo != Success");
                    Utils.Log(Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Log, out error).ToString());
                    return;
                }

                //Create the required kernel (entry function)
                Kernel kernel = Cl.CreateKernel(program, "imagingTest", out error);
                CheckError(error, "Cl.CreateKernel");

                int intPtrSize = 0;
                intPtrSize = Marshal.SizeOf(typeof(IntPtr));
                //Image's RGBA data converted to an unmanaged[] array
                byte[] inputByteArray;
                //OpenCL memory buffer that will keep our image's byte[] data.
                IMem inputImage2DBuffer;
                ImageFormat clImageFormat = new ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);
                int inputImgWidth, inputImgHeight;

                int inputImgBytesSize;
                int inputImgStride;
                //Try loading the input image
                using (FileStream imageFileStream = new FileStream(inputImagePath, FileMode.Open))
                {
                    System.Drawing.Image inputImage = System.Drawing.Image.FromStream(imageFileStream);

                    if (inputImage == null)
                    {
                        Utils.Log("Unable to load input image");
                        return;
                    }

                    inputImgWidth = inputImage.Width;
                    inputImgHeight = inputImage.Height;

                    System.Drawing.Bitmap bmpImage = new System.Drawing.Bitmap(inputImage);
                    //Get raw pixel data of the bitmap
                    //The format should match the format of clImageFormat
                    BitmapData bitmapData = bmpImage.LockBits(new Rectangle(0, 0, bmpImage.Width, bmpImage.Height),
                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb); //inputImage.PixelFormat);
                    inputImgStride = bitmapData.Stride;
                    Utils.Log($"width : {inputImgWidth}, stride : {inputImgStride}");
                    inputImgBytesSize = bitmapData.Stride * bitmapData.Height;

                    //Copy the raw bitmap data to an unmanaged byte[] array
                    inputByteArray = new byte[inputImgBytesSize];
                    Marshal.Copy(bitmapData.Scan0, inputByteArray, 0, inputImgBytesSize);
                    //Allocate OpenCL image memory buffer
                    inputImage2DBuffer = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, clImageFormat,
                        (IntPtr)bitmapData.Width, (IntPtr)bitmapData.Height,
                        (IntPtr)0, inputByteArray, out error);
                    CheckError(error, "Cl.CreateImage2D input");
                }

                //Unmanaged output image's raw RGBA byte[] array
                byte[] outputByteArray = new byte[inputImgBytesSize];
                //Allocate OpenCL image memory buffer
                IMem outputImage2DBuffer = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, clImageFormat, (IntPtr)inputImgWidth,
                    (IntPtr)inputImgHeight, (IntPtr)0, outputByteArray, out error);
                CheckError(error, "Cl.CreateImage2D output");
                //Pass the memory buffers to our kernel function
                error = Cl.SetKernelArg(kernel, 0, (IntPtr)intPtrSize, inputImage2DBuffer);
                error |= Cl.SetKernelArg(kernel, 1, (IntPtr)intPtrSize, outputImage2DBuffer);
                CheckError(error, "Cl.SetKernelArg");

                //Create a command queue, where all of the commands for execution will be added
                CommandQueue cmdQueue = Cl.CreateCommandQueue(_context, _device, (CommandQueueProperties)0, out error);
                CheckError(error, "Cl.CreateCommandQueue");
                Event clevent;
                //Copy input image from the host to the GPU.
                IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 }; //x, y, z
                IntPtr[] regionPtr = new IntPtr[] { (IntPtr)inputImgWidth, (IntPtr)inputImgHeight, (IntPtr)1 }; //x, y, z
                IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)inputImgWidth, (IntPtr)inputImgHeight, (IntPtr)1 };
                error = Cl.EnqueueWriteImage(cmdQueue, inputImage2DBuffer, Bool.True,
                    originPtr, regionPtr, (IntPtr)0, (IntPtr)0, inputByteArray, 0, null, out clevent);
                CheckError(error, "Cl.EnqueueWriteImage");
                //Execute our kernel (OpenCL code)
                error = Cl.EnqueueNDRangeKernel(cmdQueue, kernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
                CheckError(error, "Cl.EnqueueNDRangeKernel");
                //Wait for completion of all calculations on the GPU.
                error = Cl.Finish(cmdQueue);
                CheckError(error, "Cl.Finish");
                //Read the processed image from GPU to raw RGBA data byte[] array
                error = Cl.EnqueueReadImage(cmdQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr,
                    (IntPtr)0, (IntPtr)0, outputByteArray, 0, null, out clevent);
                CheckError(error, "Cl.clEnqueueReadImage");
                //Clean up memory
                Cl.ReleaseKernel(kernel);
                Cl.ReleaseCommandQueue(cmdQueue);

                Cl.ReleaseMemObject(inputImage2DBuffer);
                Cl.ReleaseMemObject(outputImage2DBuffer);
                //Get a pointer to our unmanaged output byte[] array
                GCHandle pinnedOutputArray = GCHandle.Alloc(outputByteArray, GCHandleType.Pinned);
                IntPtr outputBmpPointer = pinnedOutputArray.AddrOfPinnedObject();
                //Create a new bitmap with processed data and save it to a file.
                Bitmap outputBitmap = new Bitmap(inputImgWidth, inputImgHeight,
                    inputImgStride, PixelFormat.Format32bppArgb, outputBmpPointer);

                outputBitmap.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Png);
                pinnedOutputArray.Free();
            }
        }
    }
}