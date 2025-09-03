using System;
using System.Collections.Generic;
namespace Pass1
{
    static class Pass1
    {
        struct stLine
        {
            public string Loc;
            public string Label ;
            public string InstName;
            public string Operand;
        }

        static stLine ReadLine()
        {
            stLine Line = new stLine();

            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                Line.Label = "";
                Line.InstName = "";
                Line.Operand = "";
            }
                

            var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                Line.InstName = parts[0];
            }
            else if (parts.Length == 2)
            {
                Line.InstName = parts[0];
                Line.Operand = parts[1];
            }
            else
            {
                Line.Label = parts[0];
                Line.InstName = parts[1];
                Line.Operand = string.Join(" ", parts.Skip(2));
            }

            return Line;
        }

        static bool IsObInTheObsTable(string OBS)
        {
            HashSet<string> OBSTAB = new(StringComparer.OrdinalIgnoreCase)
                    {
                        "ADD","AND","COMP","DIV","J","JEQ","JGT","JLT","JSUB",
                        "LDA","LDCH","LDL","LDX","MUL","OR","RD","RSUB",
                        "STA","STCH","STL","STSW","STX","SUB","TD","TIX","WD"
                    };


            if (OBSTAB.Contains(OBS))
                return true;
            else
                return false;
        }

        static string AddHex(string hex, int number)
        {
            int value = Convert.ToInt32(hex, 16);

            value += number;

            return value.ToString("X");
        }

        static string GetStartingLoc(stLine StartingLine)
        {
            if (StartingLine.InstName == "START")
            {
                return StartingLine.Operand;
            }
            else
                return "0";

        }

        static void PrintLines (List<stLine> IntermediateList)
        {
            Console.Write("LOC".PadRight(4));
            Console.Write("   Label".PadRight(5));
            Console.Write("   OpCode".PadRight(5));
            Console.Write("   Operand");
            Console.WriteLine();
            for (int i = 0; i < IntermediateList.Count; i++)
            {
                Console.Write(IntermediateList[i].Loc.PadRight(4));
                if (IntermediateList[i].Label == null)
                    Console.Write("   " + " ".PadRight(5));
                else
                    Console.Write("   " + IntermediateList[i].Label.PadRight(5));

                Console.Write("   " + IntermediateList[i].InstName.PadRight(5));
                Console.Write("    " + IntermediateList[i].Operand);
                Console.WriteLine();

            }
        }



        static void AssemblerPass1()
        {
            HashSet<string> LabelsOccuredInTheCode = new HashSet<string>();
            List<stLine> IntermediateList = new List<stLine>();

            stLine Line = new stLine();
            Line = ReadLine();

            string StartingAddress = GetStartingLoc(Line);
            string LOCCTR = StartingAddress;

            Line.Loc = LOCCTR;
            IntermediateList.Add(Line);


            Line = ReadLine();

            while (Line.InstName != "END")
            {
                if (Line.Label == ".")
                    continue;


                if (LabelsOccuredInTheCode.Contains(Line.Label))
                {
                    throw new Exception("Error ! Repeated Label");
                }

                else
                {
                    Line.Loc = LOCCTR;
                    IntermediateList.Add(Line);

                    if (Line.Label != null)
                        LabelsOccuredInTheCode.Add(Line.Label);


                    if (IsObInTheObsTable(Line.InstName))
                    {
                        LOCCTR = AddHex(LOCCTR, 3);
                       
                    }
                    else if (Line.InstName == "WORD")
                    {
                        LOCCTR = AddHex(LOCCTR, 3);
                    }

                    else if (Line.InstName == "RESW")
                    {
                        int AddedToTheLoc = 3 * Convert.ToInt32(Line.Operand);
                        LOCCTR = AddHex(LOCCTR, AddedToTheLoc);
                    }

                    else if (Line.InstName == "RESB")
                    {
                        LOCCTR = AddHex(LOCCTR, Convert.ToInt32(Line.Operand));
                    }

                    else if (Line.InstName == "BYTE")
                    {
                        if (Line.Operand[0] == 'C')
                        {
                            LOCCTR = AddHex(LOCCTR, Line.Operand.Length - 3);
                        }
                        else if (Line.Operand[0] == 'X')
                        {
                            LOCCTR = AddHex(LOCCTR, (Line.Operand.Length - 3) / 2);
                        }
                        else
                        {
                            throw new Exception("Invalid Syntax for the operand ");
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid Operation ");
                    }


                }

                Line = ReadLine();

            }

            Line.Loc = LOCCTR;
            IntermediateList.Add(Line);
            PrintLines(IntermediateList);

        }

        static void Main(string[] args)
        {
             AssemblerPass1();

            
        }
    }
}