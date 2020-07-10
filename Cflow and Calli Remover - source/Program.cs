using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



    class Program
    {
        /*   
         * dnlib version 3.3.2.0
         */
        static AssemblyDef asm;
        static string asmPath;

        static void Main(string[] args)
        {
            Console.Title = "Cflow and Calli Remove - 0xCookieizi";
            try
            {
                asm = AssemblyDef.Load(args[0]);
                asmPath = args[0];          
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"not a valid PE file");
            }

        il:
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("0xCookieizi - sA#9689 , Github.com/Cooki3izi\r\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("if u wanna remove calli Write y else write n : ");
            string read = Console.ReadLine();
            if (read == "y")
            {
                Console.Clear();
                Console.Write("With calli\r\n");
                step5();//Calli Fixer
            }
            else if (read == "n")
            {
                Console.Clear();
                Console.Write("Without calli\r\n");
                step1();//Br Remover
                step2();//Br Remover
                step3();//ldloc ,ldc.i4 , ceq , brfalse Remover
                step4();//Br Remover
                step1();//Br Remover
            }
            else
            {
                goto il;
            }
            //Save the file
            ModuleWriterOptions opts = new ModuleWriterOptions(asm.ManifestModule);
            opts.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            opts.Logger = DummyLogger.NoThrowInstance;
            string path = Path.GetFileNameWithoutExtension(asmPath) + "-done.exe";
            asm.ManifestModule.Write(path, opts);
            Console.WriteLine($"\r\nSaved file in {path}");
            Console.ReadKey();
        }

        #region "CFLOW"
        static void step1()
        {
            foreach (ModuleDef module in asm.Modules)
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.HasInstructions)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                /*
                                nop
                                br
                                 */
                                if (method.Body.Instructions[i].OpCode == OpCodes.Nop && method.Body.Instructions[i + 1].OpCode == OpCodes.Br)
                                {
                                    Console.WriteLine($"Fixed - {method.Body.Instructions[i + 1]}");
                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                    }
                }
            }
        }
        static void step2()
        {
            foreach (ModuleDef module in asm.Modules)
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.HasInstructions)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                /*
                               brfalse
                               br
                                 */
                                if (method.Body.Instructions[i].OpCode == OpCodes.Brfalse && method.Body.Instructions[i + 1].OpCode == OpCodes.Br)
                                {
                                    Console.WriteLine($"Fixed - {method.Body.Instructions[i + 1]}");
                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                    }
                }
            }
        }
        static void step3()
        {
            foreach (ModuleDef module in asm.Modules)
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.HasInstructions)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                /*
                                nop
                                ldloc
                                ldc.i4
                                ceq
                                brfalse
                                 */
                                if (method.Body.Instructions[i].OpCode == OpCodes.Nop && method.Body.Instructions[i + 1].OpCode == OpCodes.Ldloc && method.Body.Instructions[i + 2].IsLdcI4() && method.Body.Instructions[i + 3].OpCode == OpCodes.Ceq && method.Body.Instructions[i + 4].OpCode == OpCodes.Brfalse)
                                {
                                    Console.WriteLine($"Fixed - {method.Body.Instructions[i + 1]}\r\nFixed - {method.Body.Instructions[i + 2]}\r\nFixed - {method.Body.Instructions[i + 3]}\r\nFixed - {method.Body.Instructions[i + 4]}");
                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i + 3].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i + 4].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                    }
                }
            }
        }
        static void step4()
        {
            foreach (ModuleDef module in asm.Modules)
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.HasInstructions)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                /*
                                ldc.i4
                                stloc	
                                br	
                                 */
                                if (method.Body.Instructions[i].IsLdcI4() && method.Body.Instructions[i + 1].OpCode == OpCodes.Stloc && method.Body.Instructions[i + 2].OpCode == OpCodes.Br)
                                {
                                    Console.WriteLine($"Fixed - {method.Body.Instructions[i]}\r\nFixed - {method.Body.Instructions[i + 1]}\r\nFixed - {method.Body.Instructions[i + 2]}");
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region "Calli Fixer"
        static void step5()
        {
            foreach (ModuleDef module in asm.Modules)
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.HasInstructions)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                /*
                                ldftn
                                calli
                                 */
                                if (method.Body.Instructions[i].OpCode == OpCodes.Ldftn && method.Body.Instructions[i + 1].OpCode == OpCodes.Calli)
                                {
                                    Console.WriteLine($"Fixed - {method.Body.Instructions[i]}\r\nFixed - {method.Body.Instructions[i + 1]}");
                                    method.Body.Instructions[i].OpCode = OpCodes.Call;
                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }

