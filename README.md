# TwoPassAssembler

# Two-Pass SIC Assembler (C#)

This is a simple educational project that implements a **Two-Pass Assembler** for the SIC architecture using C#.

- **Pass1**: Calculates addresses (LOCCTR), builds the symbol table (SYMTAB), and produces an intermediate table with: address, label, instruction, and operand.  
- **Pass2**: Generates the **Object Program** in H/T/E record format, and translates instructions and directives (`WORD`, `BYTE`, `RESW`, `RESB`) into object code.

## How it works
1. **Pass1** reads the source program line by line, computes addresses, and detects errors such as duplicate labels or invalid instructions.  
2. **Pass2** uses the results to produce the final object code with Header, Text, and End records.  

This project demonstrates the basic idea of a Two-Pass Assembler in a simplified way.
