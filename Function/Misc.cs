﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using kOS.Persistence;
using kOS.Module;
using kOS.Suffixed;
using kOS.Compilation;

namespace kOS.Function
{
    [FunctionAttribute("clearscreen")]
    public class FunctionClearScreen : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            shared.Screen.ClearScreen();
        }
    }

    [FunctionAttribute("print")]
    public class FunctionPrint : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string textToPrint = shared.Cpu.PopValue().ToString();
            shared.Screen.Print(textToPrint);
        }
    }

    [FunctionAttribute("printat")]
    public class FunctionPrintAt : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            int row = Convert.ToInt32(shared.Cpu.PopValue());
            int column = Convert.ToInt32(shared.Cpu.PopValue());
            string textToPrint = shared.Cpu.PopValue().ToString();
            shared.Screen.PrintAt(textToPrint, row, column);
        }
    }

    [FunctionAttribute("toggleflybywire")]
    public class FunctionToggleFlyByWire : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            bool enabled = Convert.ToBoolean(shared.Cpu.PopValue());
            string paramName = shared.Cpu.PopValue().ToString();
            shared.Cpu.ToggleFlyByWire(paramName, enabled);
        }
    }

    [FunctionAttribute("stage")]
    public class FunctionStage : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            Staging.ActivateNextStage();
        }
    }

    [FunctionAttribute("run")]
    public class FunctionRun : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            object volumeId = shared.Cpu.PopValue();
            string fileName = shared.Cpu.PopValue().ToString();

            if (shared.VolumeMgr != null)
            {
                if (shared.VolumeMgr.CurrentVolume != null)
                {
                    ProgramFile file = shared.VolumeMgr.CurrentVolume.GetByName(fileName);
                    if (file != null)
                    {
                        if (shared.ScriptHandler != null)
                        {
                            Stopwatch compileWatch = null;
                            bool showStatistics = Config.GetInstance().ShowStatistics;
                            if (showStatistics) compileWatch = Stopwatch.StartNew();

                            List<CodePart> parts = shared.ScriptHandler.Compile(file.Content);
                            ProgramBuilder builder = new ProgramBuilder();
                            builder.AddRange(parts);
                            List<Opcode> program = builder.BuildProgram(false);

                            if (showStatistics)
                            {
                                compileWatch.Stop();
                                shared.Cpu.TotalCompileTime += compileWatch.ElapsedMilliseconds;
                            }

                            if (volumeId != null)
                            {
                                Volume targetVolume = shared.VolumeMgr.GetVolume(volumeId);
                                if (targetVolume != null)
                                {
                                    if (shared.ProcessorMgr != null)
                                    {
                                        shared.ProcessorMgr.RunProgramOn(program, targetVolume);
                                    }
                                }
                                else
                                {
                                    throw new Exception("Volume not found");
                                }
                            }
                            else
                            {
                                shared.Cpu.RunProgram(program);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("File '{0}' not found", fileName));
                    }
                }
                else
                {
                    throw new Exception("Volume not found");
                }
            }
        }
    }

    [FunctionAttribute("add")]
    public class FunctionAddNode : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            Node node = (Node)shared.Cpu.PopValue();
            node.AddToVessel(shared.Vessel);
        }
    }

    [FunctionAttribute("remove")]
    public class FunctionRemoveNode : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            Node node = (Node)shared.Cpu.PopValue();
            node.Remove();
        }
    }

    [FunctionAttribute("logfile")]
    public class FunctionLogFile : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string fileName = shared.Cpu.PopValue().ToString();
            string expressionResult = shared.Cpu.PopValue().ToString();

            if (shared.VolumeMgr != null)
            {
                Volume volume = shared.VolumeMgr.CurrentVolume;
                if (volume != null)
                {
                    volume.AppendToFile(fileName, expressionResult);
                }
                else
                {
                    throw new Exception("Volume not found");
                }
            }
        }
    }

    [FunctionAttribute("reboot")]
    public class FunctionReboot : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            if (shared.Cpu != null) shared.Cpu.Boot();
        }
    }

    [FunctionAttribute("shutdown")]
    public class FunctionShutdown : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            if (shared.Processor != null) shared.Processor.SetMode(kOSProcessor.Modes.OFF);
        }
    }
}
