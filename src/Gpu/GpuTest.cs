using System;
using OpenCL.Net;
using PokemonSolver.Memory;

namespace PokemonSolver.Gpu
{
    public class GpuTest : GpuHandler
    {
        protected IMem? _memInput;
        protected IMem? _memOutput;

        private int _inputSize = 3;
        private int _outputSize = 3; //TODO change correct ocurrences

        public GpuTest() : base(GpuFunction.Test)
        {
        }

        protected override void AllocateMemory()
        {
            _memInput = Cl.CreateBuffer(GpuContext, MemFlags.ReadOnly, sizeof(int) * _inputSize, out ErrorCode);
            if (ErrorCode != ErrorCode.Success)
            {
                throw new Exception("unable to create mem input buffer");
            }

            _memOutput = Cl.CreateBuffer(GpuContext, MemFlags.ReadOnly, sizeof(int) * _inputSize, out ErrorCode);
            if (ErrorCode != ErrorCode.Success)
            {
                throw new Exception("unable to create mem output buffer");
            }
        }

        public int[] Execute(int[] data)
        {
            AllocateMemory();

            Event event0;

            var results = new int[data.Length];
            
            var local = new InfoBuffer(new IntPtr(IntPtr.Size));
            Cl.GetKernelWorkGroupInfo(Kernel, Gpu, KernelWorkGroupInfo.WorkGroupSize, new IntPtr(sizeof(int)), local, out _);
            Cl.EnqueueWriteBuffer(CommandQueue, _memInput, Bool.True, IntPtr.Zero, new IntPtr(sizeof(int) * _inputSize), data, 0, null, out event0);
            Cl.SetKernelArg(Kernel, 0, new IntPtr(IntPtr.Size), _memInput);
            Cl.SetKernelArg(Kernel, 1, new IntPtr(IntPtr.Size), _memOutput);
            var workGroupSizePtr = new IntPtr[] { new(_inputSize) };
            Cl.EnqueueNDRangeKernel(CommandQueue, Kernel, 1, null, workGroupSizePtr, null, 0, null, out event0);
            Cl.Finish(CommandQueue);
            Cl.EnqueueReadBuffer(CommandQueue, _memOutput, Bool.True, IntPtr.Zero, new IntPtr(sizeof(int) * _inputSize), results, 0, null, out event0);
            return results;
        }
    }
}