using System;
using OpenCL.Net;
using PokemonSolver.Memory;

namespace PokemonSolver.Gpu
{
    public class GpuFlood : GpuHandler
    {
        public GpuFlood() : base(GpuFunction.Flood)
        {
        }

        protected IMem? _memInput;
        protected IMem? _permissionBytes;
        protected IMem? _memOutput;
        protected IMem? _memContinueCriteria;

        private int _memInputSize;
        private int _memPermissionSize;
        private int _memOutputSize;

        protected override void AllocateMemory()
        {
            _memInput = Cl.CreateBuffer(GpuContext, MemFlags.ReadOnly, _memInputSize, out ErrorCode);
            if (ErrorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create memory input buffer");
                return;
            }

            _permissionBytes = Cl.CreateBuffer(GpuContext, MemFlags.ReadOnly, _memPermissionSize, out ErrorCode);
            if (ErrorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create permission bytes buffer");
                return;
            }

            _memOutput = Cl.CreateBuffer(GpuContext, MemFlags.WriteOnly, _memOutputSize, out ErrorCode);
            if (ErrorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create memory output buffer");
                return;
            }

            _memContinueCriteria = Cl.CreateBuffer(GpuContext, MemFlags.ReadWrite, sizeof(byte), out ErrorCode);
            if (ErrorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create memory continuation criterium");
                return;
            }
        }

        public int[] Execute(int[] data, byte[] permissionBytes, int width, int xGoal, int yGoal)
        {
            var height = data.Length / width;
            Utils.Log($"width= {width}, height= {height}");
            // AllocateMemory(width * height);
            _memInputSize = _memPermissionSize = _memOutputSize = sizeof(int) * width * height;

            AllocateMemory();
            Event event0;

            IntPtr _;
            var local = new InfoBuffer(new IntPtr(IntPtr.Size));
            Cl.GetKernelWorkGroupInfo(Kernel, Gpu, KernelWorkGroupInfo.WorkGroupSize, new IntPtr(sizeof(int)), local, out _);
            Cl.EnqueueWriteBuffer(CommandQueue, _permissionBytes, Bool.True, IntPtr.Zero, new IntPtr(_memPermissionSize), permissionBytes, 0, null,
                out event0);

            Cl.SetKernelArg(Kernel, 0, new IntPtr(IntPtr.Size), _memInput);
            Cl.SetKernelArg(Kernel, 1, new IntPtr(IntPtr.Size), _memOutput);
            Cl.SetKernelArg(Kernel, 2, new IntPtr(IntPtr.Size), _permissionBytes);
            Cl.SetKernelArg(Kernel, 3, new IntPtr(IntPtr.Size), _memContinueCriteria);
            Cl.SetKernelArg(Kernel, 4, new IntPtr(sizeof(int)), width);
            Cl.SetKernelArg(Kernel, 5, new IntPtr(sizeof(int)), height);
            Cl.SetKernelArg(Kernel, 6, new IntPtr(sizeof(int)), xGoal);
            Cl.SetKernelArg(Kernel, 7, new IntPtr(sizeof(int)), yGoal);

            var workGroupSizePtr = new IntPtr[] { new(width * height) };
            var continueCriterium = new byte[] { 1 };

            int index = 0;
            while (continueCriterium[0] != 0 && index++ < width * height)
            {
                continueCriterium[0] = 0;
                Cl.EnqueueWriteBuffer(CommandQueue, _memContinueCriteria, Bool.True, IntPtr.Zero, new IntPtr(sizeof(byte)), continueCriterium, 0, null,
                    out event0);
                Cl.EnqueueWriteBuffer(CommandQueue, _memInput, Bool.True, IntPtr.Zero, new IntPtr(sizeof(int) * width * height), data, 0, null, out event0);

                Cl.EnqueueNDRangeKernel(CommandQueue, Kernel, 1, null, workGroupSizePtr, null, 0, null, out event0);
                // Cl.EnqueueTask(_commandQueue, _kernel, 0, null, out event0);
                Cl.Finish(CommandQueue);
                Cl.EnqueueReadBuffer(CommandQueue, _memOutput, Bool.True, IntPtr.Zero, new IntPtr(sizeof(int) * width * height), data, 0, null, out event0);
                Cl.EnqueueReadBuffer(CommandQueue, _memContinueCriteria, Bool.True, 0, sizeof(byte), continueCriterium, 0, null, out event0);
                // Utils.Log($"continue ? {continueCriterium[0]}");
            }

            Utils.Log($"Iterations : {index}");

            return data;
        }
    }
}