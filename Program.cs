using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.PE;
using dnlib.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.Diagnostics.CodeAnalysis;

namespace AdultOrigamiUnpacker
{
    class Program
    {
        public static ModuleDefMD asm;
        public static IPEImage peFile;
        public static string path;
        public static List<ImageSectionHeader> susSections = new List<ImageSectionHeader>();

        static void Main(string[] args)
        {
            Console.Title = "PaperPlane";
            Console.WriteLine("PaperPlane by misonothx | Yet Another Origami Unpacker (supports XOR mods)");
            Console.WriteLine(" |- https://github.com/miso-xyz/PaperPlane");
            Console.WriteLine(" |- https://github.com/dr4k0nia/Origami");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Note: don't mind the amogus jokes i was bored");
            Console.ResetColor();
            Console.WriteLine();
            path = args[0];
            asm = ModuleDefMD.Load(args[0]);
            peFile = asm.Metadata.PEImage;
            if (!hasPESection())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not a valid Origami-Protected application! (or is not supported)");
            }
            if (susSections.Count != 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Found " + susSections.Count + " Sussy Section(s)!");
                if (isModded()) { Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine("Packer Used: Modded Origami"); }
                Console.WriteLine();
                dumpSections();
            }
            Console.ResetColor();
            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        public static void dumpSections()
        {
            if (susSections.Count != 0)
            {
                foreach (ImageSectionHeader section in susSections)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Now Voting For '" + section.DisplayName + "'!");
                    byte[] sectionData = peFile.CreateReader(section.VirtualAddress, section.SizeOfRawData).ToArray();
                    if (isModded())
                    {
                        try
                        {
                            byte[] unXored = new byte[section.SizeOfRawData];
                            for (int x = 0; x < section.SizeOfRawData; x++)
                            {
                                unXored[x] = Convert.ToByte(Convert.ToChar(sectionData[x]) ^ asm.EntryPoint.Name.ToString()[x % asm.EntryPoint.Name.ToString().Length]);
                            }
                            Directory.CreateDirectory("PaperPlane\\" + Path.GetFileName(path));
                            File.WriteAllBytes(Path.GetFileName(path) + "-" + section.DisplayName + "-PaperPlane.bin", Decompress(unXored));
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(section.DisplayName + "' was the imposter and was XORed!");
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("Failed to decrypt '" + section.DisplayName + "'");
                            File.WriteAllBytes(Path.GetFileName(path) + "-" + section.DisplayName + "-PaperPlane.bin", Decompress(sectionData));
                        }
                    }
                    else
                    {
                        File.WriteAllBytes(Path.GetFileName(path) + "-" + section.DisplayName + "-PaperPlane.bin", Decompress(sectionData));
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(section.DisplayName + "' was the imposter!");
                    }
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Saved as '" + Path.GetFileName(path) + "-" + section.DisplayName + "-PaperPlane.bin'!");
                    Console.WriteLine();
                }
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            using (DeflateStream ds = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
            {
                ds.CopyTo(ms);
            }
            return ms.ToArray();
        }

        public static bool isModded()
        {
            try { Guid.Parse(asm.EntryPoint.Name); return true; }
            catch { return false; }
        }

        public static bool hasPESection()
        {
            bool susFound = false;
            foreach (ImageSectionHeader section in peFile.ImageSectionHeaders)
            {
                switch (section.DisplayName)
                {
                    case ".text":
                    case ".rsrc":
                    case ".reloc":
                        continue;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[!] Sussy PE Section Found ('" + section.DisplayName + "')");
                        susSections.Add(section);
                        susFound = true;
                        break;
                }
            }
            return susFound;
        }
    }
}