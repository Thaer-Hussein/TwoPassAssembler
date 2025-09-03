using System;
using System.Collections.Generic;
using System.Linq;

namespace Pass2
{
    public static class clsSICInfo
    {
        public static readonly Dictionary<string, string> DirectivesWithNoObjectCode = new Dictionary<string, string>
    {
        {"START", ""},
        {"END", ""},
        {"EQU", ""},
        {"ORG", ""},
        {"RESW", ""},
        {"RESB", ""},
        {"LTORG", ""},
    };

        public static readonly Dictionary<string, string> OpcodesFixed = new Dictionary<string, string>
    {
        {"LDA", "00"},
        {"LDX", "04"},
        {"LDL", "08"},
        {"STA", "0C"},
        {"ADD", "18"},
        {"SUB", "1C"},
        {"MUL", "20"},
        {"DIV", "24"},
        {"COMP", "28"},
        {"TIX", "2C"},
        {"JEQ", "30"},
        {"JGT", "34"},
        {"JLT", "38"},
        {"J", "3C"},
        {"JSUB", "48"},
        {"RSUB", "4C"},
        {"LDCH", "50"},
        {"STCH", "54"}
    };

        public static readonly HashSet<string> DataDirectives = new HashSet<string>
    {
        {"BYTE"},
        {"WORD"},
        {"LITERAL"}
    };

        public static Dictionary<string, string> LocPerLabel = new Dictionary<string, string>();
    }

    public static class clsObjectProgram
    {
        static string HeaderRecord;

        //this stores the text record before pushing 
        static string tempTextRecord;

        static List<string> TextRecords = new List<string>(); // Initialize the list

        static string EndRecord;

        public static void SetHeaderRecord_H()
        {
            HeaderRecord = "H";
        }

        public static void SetHeaderRecordProgramName(string ProgramName)
        {

            HeaderRecord = HeaderRecord + ProgramName;

            for (int i = 0; i < 6 - ProgramName.Length; i++)
            {
                HeaderRecord += " ";
            }
        }

        public static void SetHeaderRecordStartLoc(string Loc)
        {
            HeaderRecord += ("00" + Loc);
        }

        public static void SetHeaderRecordProgramLength(string StartLoc, string EndLoc)
        {
            int start = Convert.ToInt32(StartLoc, 16);
            int end = Convert.ToInt32(EndLoc, 16);
            int length = end - start;
            HeaderRecord += length.ToString("X6");
        }

        public static void SetTextRecord_T()
        {
            tempTextRecord = "T";
        }

        public static void SetTextRecordStartLoc(string StartLoc)
        {
            tempTextRecord += StartLoc.PadLeft(6, '0');
        }

        public static void SetTextRecordLength(int Length)
        {
            string LengthInHexa = clsGeneralHexaFunctions.IntToHex(Length);
            tempTextRecord += LengthInHexa.PadLeft(2, '0');
        }

        public static void AddLocsToTextRecord(ref List<string> Locs)
        {
            for (int i = 0; i < Locs.Count(); i++)
            {
                tempTextRecord += Locs[i];
            }
        }

        public static void PushRecordToRecords()
        {
            TextRecords.Add(tempTextRecord);
        }


        public static void SetEndRecord(string FirstExecutableAddress)
        {
            EndRecord = "E" + FirstExecutableAddress.PadLeft(6, '0');
        }

        public static void PrintObjectProgram()
        {
            Console.WriteLine("Object Program:");
            Console.WriteLine(HeaderRecord);
            foreach (string textRecord in TextRecords)
            {
                Console.WriteLine(textRecord);
            }
            Console.WriteLine(EndRecord);
        }
    }
    public class clsLine
    {
        public string Loc { set; get; }
        public string Label { set; get; }
        public string InstName { set; get; }
        public string Operand { set; get; }
        public string ObjectCode { set; get; }

        public clsLine(string Loc, string Label, string InstName, string Operand)
        {
            this.Loc = Loc;
            this.Label = Label;
            this.InstName = InstName;
            this.Operand = Operand;
        }
    }

    public static class clsGeneralHexaFunctions
    {
        public static string IntToHex(int number)
        {
            return number.ToString("X");
        }


        public static string CharToHex(char c)
        {
            return ((int)c).ToString("X2");
        }

        public static string ProcessByteOperand(string operand)
        {
            if (operand.StartsWith("C'") && operand.EndsWith("'"))
            {

                string chars = operand.Substring(2, operand.Length - 3);
                string result = "";
                foreach (char c in chars)
                {
                    result += CharToHex(c);
                }
                return result;
            }
            else if (operand.StartsWith("X'") && operand.EndsWith("'"))
            {

                return operand.Substring(2, operand.Length - 3);
            }
            return operand;
        }
    }
    static public class clsGeneralFunctionsOnTheLine
    {
        static public clsLine ReadLine()
        {

            Console.Write("Enter line: ");
            string line = Console.ReadLine();

            clsLine result = new clsLine("", "", "", "");

            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 4)
            {

                result.Loc = parts[0];
                result.Label = parts[1];
                result.InstName = parts[2];
                result.Operand = parts[3];
            }
            else if (parts.Length == 3)
            {
                result.Loc = parts[0];

                result.InstName = parts[1];
                result.Operand = parts[2];
            }
            else if (parts.Length == 2)
            {
                result.Loc = parts[0];
                result.InstName = parts[1];
            }
            else
            {
                throw new Exception("The prev line is not valid .... please re enter another valid line");
            }

            return result;
        }

        static public void FindTheOpCodeValue(string InstName, ref string Opcode)
        {
            if (clsSICInfo.DirectivesWithNoObjectCode.ContainsKey(InstName))
                Opcode = clsSICInfo.DirectivesWithNoObjectCode[InstName];

            else if (clsSICInfo.OpcodesFixed.ContainsKey(InstName))
                Opcode = clsSICInfo.OpcodesFixed[InstName];

        }
    }

    static public class Pass
    {
        static List<clsLine> Lines = new List<clsLine>();
        static public void RunPassTwo()
        {
            clsLine Line = clsGeneralFunctionsOnTheLine.ReadLine();
            string StartAddress = "";
            string EndAddress = "";

            if (Line.InstName == "START")
            {
                StartAddress = Line.Loc;
                clsSICInfo.LocPerLabel.Add(Line.Label, Line.Loc);
                clsObjectProgram.SetHeaderRecord_H();
                clsObjectProgram.SetHeaderRecordProgramName(Line.Label);
                clsObjectProgram.SetHeaderRecordStartLoc(Line.Loc);

                Lines.Add(Line);

                Line = clsGeneralFunctionsOnTheLine.ReadLine();

                List<string> LocsInTextRecord = new List<string>();
                string StartingLocinTextRecord = Line.Loc;
                int LocsLength = 0;
                bool Flag = true;

                while (Line.InstName != "END")
                {

                    if (Flag)
                    {
                        StartingLocinTextRecord = Line.Loc;
                        Flag = !Flag;
                    }


                    if (Line.Label != "." && Line.Label != "")
                    {
                        string OpCode = null;
                        clsGeneralFunctionsOnTheLine.FindTheOpCodeValue(Line.InstName, ref OpCode);


                        if (!string.IsNullOrEmpty(Line.Label))
                        {
                            clsSICInfo.LocPerLabel.Add(Line.Label, Line.Loc);
                        }

                        if (OpCode != null)
                        {
                            if (OpCode == "")
                            {
                                Line.ObjectCode = "";
                                Lines.Add(Line);

                                if (LocsInTextRecord.Count() == 0)
                                {
                                    Flag = true;
                                }
                                else
                                {
                                    clsObjectProgram.SetTextRecord_T();
                                    clsObjectProgram.SetTextRecordStartLoc(StartingLocinTextRecord);

                                    clsObjectProgram.SetTextRecordLength(LocsLength / 2);
                                    clsObjectProgram.AddLocsToTextRecord(ref LocsInTextRecord);
                                    clsObjectProgram.PushRecordToRecords();

                                    LocsInTextRecord.Clear();
                                    LocsLength = 0;
                                    Flag = true;
                                }
                            }
                            else
                            {
                                Line.ObjectCode = OpCode;

                                if (Line.Operand == "" || Line.Operand == null)
                                {
                                    Line.ObjectCode += "0000";
                                }
                                else
                                {
                                    if (clsSICInfo.LocPerLabel.ContainsKey(Line.Operand))
                                    {
                                        Line.ObjectCode += clsSICInfo.LocPerLabel[Line.Operand].PadLeft(4, '0');
                                    }
                                    else
                                    {
                                        Line.ObjectCode += "0000";
                                        throw new Exception("Undefined Symbol: " + Line.Operand);
                                    }
                                }

                                Lines.Add(Line);

                                LocsInTextRecord.Add(Line.ObjectCode);
                                LocsLength += Line.ObjectCode.Length;

                                if (LocsLength >= 60) // Use >= to handle cases where it exceeds 60
                                {
                                    clsObjectProgram.SetTextRecord_T();
                                    clsObjectProgram.SetTextRecordStartLoc(StartingLocinTextRecord);
                                    //here divided by two because the length is required in bytes
                                    clsObjectProgram.SetTextRecordLength(LocsLength / 2);
                                    clsObjectProgram.AddLocsToTextRecord(ref LocsInTextRecord);
                                    clsObjectProgram.PushRecordToRecords();

                                    LocsInTextRecord.Clear();
                                    LocsLength = 0;
                                    Flag = true;
                                }
                            }
                        }
                        else
                        {
                            if (clsSICInfo.DataDirectives.Contains(Line.InstName))
                            {
                                // Handle data directives
                                if (Line.InstName == "WORD")
                                {
                                    // Convert decimal to 6-digit hex
                                    int value = Convert.ToInt32(Line.Operand);
                                    Line.ObjectCode = value.ToString("X6");
                                }
                                else if (Line.InstName == "BYTE")
                                {
                                    Line.ObjectCode = clsGeneralHexaFunctions.ProcessByteOperand(Line.Operand);
                                }

                                Lines.Add(Line);

                                if (!string.IsNullOrEmpty(Line.ObjectCode))
                                {
                                    LocsInTextRecord.Add(Line.ObjectCode);
                                    LocsLength += Line.ObjectCode.Length;

                                    if (LocsLength >= 60)
                                    {
                                        clsObjectProgram.SetTextRecord_T();
                                        clsObjectProgram.SetTextRecordStartLoc(StartingLocinTextRecord);
                                        clsObjectProgram.SetTextRecordLength(LocsLength / 2);
                                        clsObjectProgram.AddLocsToTextRecord(ref LocsInTextRecord);
                                        clsObjectProgram.PushRecordToRecords();

                                        LocsInTextRecord.Clear();
                                        LocsLength = 0;
                                        Flag = true;
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("Undefined Instruction: " + Line.InstName);
                            }
                        }
                    }
                    else
                    {
                        // Handle lines without labels
                        string OpCode = null;
                        clsGeneralFunctionsOnTheLine.FindTheOpCodeValue(Line.InstName, ref OpCode);

                        if (OpCode != null)
                        {
                            if (OpCode == "")
                            {
                                Line.ObjectCode = "";
                                Lines.Add(Line);

                                if (LocsInTextRecord.Count() == 0)
                                {
                                    Flag = true;
                                }
                                else
                                {
                                    clsObjectProgram.SetTextRecord_T();
                                    clsObjectProgram.SetTextRecordStartLoc(StartingLocinTextRecord);
                                    clsObjectProgram.SetTextRecordLength(LocsLength / 2);
                                    clsObjectProgram.AddLocsToTextRecord(ref LocsInTextRecord);
                                    clsObjectProgram.PushRecordToRecords();

                                    LocsInTextRecord.Clear();
                                    LocsLength = 0;
                                    Flag = true;
                                }
                            }
                            else
                            {
                                Line.ObjectCode = OpCode;

                                if (string.IsNullOrEmpty(Line.Operand))
                                {
                                    Line.ObjectCode += "0000";
                                }
                                else
                                {
                                    if (clsSICInfo.LocPerLabel.ContainsKey(Line.Operand))
                                    {
                                        Line.ObjectCode += clsSICInfo.LocPerLabel[Line.Operand].PadLeft(4, '0');
                                    }
                                    else
                                    {
                                        Line.ObjectCode += "0000";
                                        throw new Exception("Undefined Symbol: " + Line.Operand);
                                    }
                                }

                                Lines.Add(Line);

                                LocsInTextRecord.Add(Line.ObjectCode);
                                LocsLength += Line.ObjectCode.Length;

                                if (LocsLength >= 60)
                                {
                                    clsObjectProgram.SetTextRecord_T();
                                    clsObjectProgram.SetTextRecordStartLoc(StartingLocinTextRecord);
                                    clsObjectProgram.SetTextRecordLength(LocsLength / 2);
                                    clsObjectProgram.AddLocsToTextRecord(ref LocsInTextRecord);
                                    clsObjectProgram.PushRecordToRecords();

                                    LocsInTextRecord.Clear();
                                    LocsLength = 0;
                                    Flag = true;
                                }
                            }
                        }
                        else if (clsSICInfo.DataDirectives.Contains(Line.InstName))
                        {
                            if (Line.InstName == "WORD")
                            {
                                int value = Convert.ToInt32(Line.Operand);
                                Line.ObjectCode = value.ToString("X6");
                            }
                            else if (Line.InstName == "BYTE")
                            {
                                Line.ObjectCode = clsGeneralHexaFunctions.ProcessByteOperand(Line.Operand);
                            }

                            Lines.Add(Line);

                            if (!string.IsNullOrEmpty(Line.ObjectCode))
                            {
                                LocsInTextRecord.Add(Line.ObjectCode);
                                LocsLength += Line.ObjectCode.Length;

                                if (LocsLength >= 60)
                                {
                                    clsObjectProgram.SetTextRecord_T();
                                    clsObjectProgram.SetTextRecordStartLoc(StartingLocinTextRecord);
                                    clsObjectProgram.SetTextRecordLength(LocsLength / 2);
                                    clsObjectProgram.AddLocsToTextRecord(ref LocsInTextRecord);
                                    clsObjectProgram.PushRecordToRecords();

                                    LocsInTextRecord.Clear();
                                    LocsLength = 0;
                                    Flag = true;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Undefined Instruction: " + Line.InstName);
                        }
                    }

                    Line = clsGeneralFunctionsOnTheLine.ReadLine();
                }

                // Handle END directive
                EndAddress = Line.Operand;
                Lines.Add(Line);

                if (LocsInTextRecord.Count() > 0)
                {
                    clsObjectProgram.SetTextRecord_T();
                    clsObjectProgram.SetTextRecordStartLoc(StartingLocinTextRecord);
                    clsObjectProgram.SetTextRecordLength(LocsLength / 2);
                    clsObjectProgram.AddLocsToTextRecord(ref LocsInTextRecord);
                    clsObjectProgram.PushRecordToRecords();
                }

                // Set program length in header record
                clsObjectProgram.SetHeaderRecordProgramLength(StartAddress, Line.Loc);

                // Create end record
                clsObjectProgram.SetEndRecord(string.IsNullOrEmpty(EndAddress) ? StartAddress : EndAddress);

                // Print the complete object program
                clsObjectProgram.PrintObjectProgram();
            }
            else
            {
                throw new Exception("Program must start with START directive");
            }
        }
    }
    public class Pass2
    {
        public static void Main(string[] args)
        {
            try
            {
                Pass.RunPassTwo();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}