using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BizHawk.Common;
using OpenCL.Net;
using PokemonSolver.Memory;
using Environment = System.Environment;

namespace PokemonSolver.Gpu
{
    public class GpuHandler
    {
        private const string FunctionName = "flood";

        // public const int BufferSize = 100;
        private Device _gpu;
        private ErrorCode _errorCode;
        private Context _gpuContext;
        private CommandQueue _commandQueue;
        private Kernel _kernel;
        private IMem? _memInput;
        private IMem? _permissionBytes;
        private IMem? _memOutput;
        private IMem? _memContinueCriteria;

        public GpuHandler()
        {
            var platforms = Cl.GetPlatformIDs(out _errorCode);
            var devices = new List<Device>();
            var devicesNames = new List<string>();

            foreach (var platform in platforms)
            {
                var deviceName = Cl.GetPlatformInfo(platform, PlatformInfo.Name, out _errorCode).ToString();

                //GUI : List box du formulaire au besoin
                //Lsb_Devices_Names.Items.Add(Device_Name);

                //GPU uniquement 
                devices.AddRange(Cl.GetDeviceIDs(platform, DeviceType.Gpu, out _errorCode));
                devicesNames.Add(deviceName);
            }

            if (devices.Count <= 0)
                throw new Exception("No CUDA GPU");


            foreach (var device in devices)
            {
                Utils.Log($"Device : {device.ToString()}");
            }

            foreach (var deviceName in devicesNames)
            {
                Utils.Log($"Device name : {deviceName}");
            }

            _gpu = devices[0]; //arbitrary, I know
            _gpuContext = Cl.CreateContext(null, 1, new Device[] { _gpu }, null, IntPtr.Zero, out _errorCode);
            if (_errorCode != ErrorCode.Success)
            {
                Utils.Error("Can't create context");
                return;
            }

            _commandQueue = Cl.CreateCommandQueue(_gpuContext, _gpu, CommandQueueProperties.OutOfOrderExecModeEnable, out _errorCode);
            if (_errorCode != ErrorCode.Success)
            {
                Utils.Error("Can't create command queue");
                return;
            }

            var program = Compile("multiply");

            _kernel = Cl.CreateKernel(program, FunctionName, out _errorCode);
            if (_errorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create kernel");
                return;
            }
        }

        private Program Compile(string path)
        {
            path = $"{Environment.CurrentDirectory}\\..\\src\\Gpu\\HLSL\\{path}.cl";

            if (!File.Exists(path))
                throw new Exception($"No openCL program found at path {path}");

            var source = File.ReadAllText(path);

            var program = Cl.CreateProgramWithSource(_gpuContext, 1, new[] { source }, null, out _errorCode);
            Cl.BuildProgram(program, 0, null, string.Empty, null, IntPtr.Zero);

            if (Cl.GetProgramBuildInfo(program, _gpu, ProgramBuildInfo.Status, out _errorCode).CastTo<BuildStatus>() != BuildStatus.Success)
            {
                if (_errorCode != ErrorCode.Success)
                {
                    Utils.Error($"ERROR : Cl.GetProgramBuildInfo ({_errorCode})");
                    Utils.Error(Cl.GetProgramBuildInfo(program, _gpu, ProgramBuildInfo.Log, out _errorCode).ToString());
                }

                throw new Exception("Unable to build CUDA program");
            }

            return program;
        }

        private void AllocateMemory(int size)
        {
            _memInput = Cl.CreateBuffer(_gpuContext, MemFlags.ReadOnly, sizeof(int) * size, out _errorCode);
            if (_errorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create memory input buffer");
                return;
            }

            _permissionBytes = Cl.CreateBuffer(_gpuContext, MemFlags.ReadOnly, sizeof(int) * size, out _errorCode);
            if (_errorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create permission bytes buffer");
                return;
            }

            _memOutput = Cl.CreateBuffer(_gpuContext, MemFlags.WriteOnly, sizeof(int) * size, out _errorCode);
            if (_errorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create memory output buffer");
                return;
            }

            _memContinueCriteria = Cl.CreateBuffer(_gpuContext, MemFlags.ReadWrite, sizeof(byte), out _errorCode);
            if (_errorCode != ErrorCode.Success)
            {
                Utils.Error("unable to create memory continuation criterium");
                return;
            }
        }

        public int[] Execute(int[] data, byte[] permissionBytes, int width, int xGoal, int yGoal)
        {
            // var width = data.GetLength(0);
            // var height = data.GetLength(1);
            var height = data.Length / width;
            Utils.Log($"width= {width}, height= {height}");
            AllocateMemory(width * height);
            Event event0;

            IntPtr _;
            var local = new InfoBuffer(new IntPtr(IntPtr.Size));
            Cl.GetKernelWorkGroupInfo(_kernel, _gpu, KernelWorkGroupInfo.WorkGroupSize, new IntPtr(sizeof(int)), local, out _);
            Cl.EnqueueWriteBuffer(_commandQueue, _permissionBytes, Bool.True, IntPtr.Zero, new IntPtr(sizeof(byte) * width * height), permissionBytes, 0, null, out event0);

            Cl.SetKernelArg(_kernel, 0, new IntPtr(IntPtr.Size), _memInput);
            Cl.SetKernelArg(_kernel, 1, new IntPtr(IntPtr.Size), _memOutput);
            Cl.SetKernelArg(_kernel, 2, new IntPtr(IntPtr.Size), _permissionBytes);
            Cl.SetKernelArg(_kernel, 3, new IntPtr(IntPtr.Size), _memContinueCriteria);
            Cl.SetKernelArg(_kernel, 4, new IntPtr(sizeof(int)), width);
            Cl.SetKernelArg(_kernel, 5, new IntPtr(sizeof(int)), height);
            Cl.SetKernelArg(_kernel, 6, new IntPtr(sizeof(int)), xGoal);
            Cl.SetKernelArg(_kernel, 7, new IntPtr(sizeof(int)), yGoal);

            var workGroupSizePtr = new IntPtr[] { new(width * height) };
            var continueCriterium = new byte[] { 1 };

            int index = 0;
            while (continueCriterium[0] != 0 && index++ < width * height)
            {
                continueCriterium[0] = 0;
                Cl.EnqueueWriteBuffer(_commandQueue, _memContinueCriteria, Bool.True, IntPtr.Zero, new IntPtr(sizeof(byte)), continueCriterium, 0, null,
                    out event0);
                Cl.EnqueueWriteBuffer(_commandQueue, _memInput, Bool.True, IntPtr.Zero, new IntPtr(sizeof(int) * width * height), data, 0, null, out event0);

                Cl.EnqueueNDRangeKernel(_commandQueue, _kernel, 1, null, workGroupSizePtr, null, 0, null, out event0);
                // Cl.EnqueueTask(_commandQueue, _kernel, 0, null, out event0);
                Cl.Finish(_commandQueue);
                Cl.EnqueueReadBuffer(_commandQueue, _memOutput, Bool.True, IntPtr.Zero, new IntPtr(sizeof(int) * width * height), data, 0, null, out event0);
                Cl.EnqueueReadBuffer(_commandQueue, _memContinueCriteria, Bool.True, 0, sizeof(byte), continueCriterium, 0, null, out event0);
                // Utils.Log($"continue ? {continueCriterium[0]}");
            }

            Utils.Log($"Iterations : {index}");

            return data;
            // for (int y = 0; y < height; y++)
            // for (int x = 0; x < width; x++)
            //     resultsPlop[x, y] = results[y * width + x];
            // return resultsPlop;
        }
    }
}