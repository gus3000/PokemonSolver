using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenCL.Net;
using PokemonSolver.Memory;
using Environment = System.Environment;

namespace PokemonSolver.Gpu
{
    public abstract class GpuHandler
    {
        protected static readonly int IntPtrSize = Marshal.SizeOf(typeof(IntPtr));
        
        protected readonly string _functionName;
        private uint _index;

        // public const int BufferSize = 100;
        protected Device Gpu;
        protected ErrorCode ErrorCode;
        protected Context GpuContext;
        protected CommandQueue CommandQueue;
        protected Kernel Kernel;

        protected GpuHandler(string functionName)
        {
            _functionName = functionName;

            Init();
            var program = Compile();

            Kernel = Cl.CreateKernel(program, _functionName, out ErrorCode);
            _index = 0;
            CheckError("Kernel creation");
        }

        protected void CheckError(ErrorCode err, string name)
        {
            if (err != ErrorCode.Success)
            {
                throw new Exception($"ERROR: {name} ({err})");
            }
        }

        protected void CheckError(string name)
        {
            CheckError(ErrorCode, name);
        }

        protected void ContextNotify(string errInfo, byte[] data, IntPtr cb, IntPtr userData)
        {
            Utils.Log($"OpenCL Notification: {errInfo}");
        }

        protected ErrorCode AddArg(IntPtr argsize, object argValue)
        {
            return Cl.SetKernelArg(Kernel, _index++, argsize, argValue);
        }

        protected ErrorCode AddArg(int argsize, object argValue)
        {
            return AddArg((IntPtr)argsize, argValue);
        }
        
        private void Init()
        {
            var platforms = Cl.GetPlatformIDs(out ErrorCode);
            var devices = new List<Device>();
            var devicesNames = new List<string>();

            foreach (var platform in platforms)
            {
                var deviceName = Cl.GetPlatformInfo(platform, PlatformInfo.Name, out ErrorCode).ToString();

                //GUI : List box du formulaire au besoin
                //Lsb_Devices_Names.Items.Add(Device_Name);

                //GPU uniquement 
                devices.AddRange(Cl.GetDeviceIDs(platform, DeviceType.Gpu, out ErrorCode));
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

            Gpu = devices[0]; //arbitrary, I know
            GpuContext = Cl.CreateContext(null, 1, new[] { Gpu }, null, IntPtr.Zero, out ErrorCode);
            CheckError("Context creation");

            CommandQueue = Cl.CreateCommandQueue(GpuContext, Gpu, CommandQueueProperties.OutOfOrderExecModeEnable, out ErrorCode);
            CheckError("Command queue creation");
        }

        private Program Compile()
        {
            var path = $"{Environment.CurrentDirectory}/../src/Gpu/HLSL/{_functionName}.cl";

            if (!File.Exists(path))
                throw new Exception($"No openCL program found at path {path}");

            var source = File.ReadAllText(path);

            var program = Cl.CreateProgramWithSource(GpuContext, 1, new[] { source }, null, out ErrorCode);
            Cl.BuildProgram(program, 0, null, string.Empty, null, IntPtr.Zero);

            if (Cl.GetProgramBuildInfo(program, Gpu, ProgramBuildInfo.Status, out ErrorCode).CastTo<BuildStatus>() != BuildStatus.Success)
            {
                if (ErrorCode != ErrorCode.Success)
                {
                    Utils.Error($"ERROR : Cl.GetProgramBuildInfo ({ErrorCode})");
                    Utils.Error(Cl.GetProgramBuildInfo(program, Gpu, ProgramBuildInfo.Log, out ErrorCode).ToString());
                }

                throw new Exception("Unable to build CUDA program");
            }

            return program;
        }

        protected abstract void AllocateMemory();
    }
}