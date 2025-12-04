#!/usr/bin/env python3
"""
Generate win16-api-status.json from Win16 module source files.

This script parses the Win16 module C# files to extract supported function names
from the switch statements in the TryInvokeWin16 methods.

Usage:
    python3 generate-win16-api-status.py

Output:
    Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/win16-api-status.json
"""

import re
import json
from pathlib import Path

def extract_win16_functions():
    """Extract Win16 functions from module source files."""
    modules = {}
    
    # Parse each Win16 module file
    win16_dir = Path("Win32Emu/Win32/Win16")
    if not win16_dir.exists():
        print(f"Error: Directory {win16_dir} not found")
        return modules
    
    for file_path in win16_dir.glob("*.cs"):
        content = file_path.read_text()
        
        # Split content by class definitions to handle multiple classes per file
        # Pattern matches: class name, module name from Name property, and switch body
        class_pattern = (
            r'internal class (Win16\w+Module).*?'
            r'public string Name => "([^"]+)".*?'
            r'public override bool TryInvokeWin16\(.*?\)\s*\{(.*?)'
            r'(?=internal class|$)'
        )
        
        for match in re.finditer(class_pattern, content, re.DOTALL):
            class_name = match.group(1)
            module_name = match.group(2) + ".DLL"
            method_body = match.group(3)
            
            # Extract all case statements from the method body
            case_pattern = r'case\s+"([^"]+)":'
            functions = set(re.findall(case_pattern, method_body))
            
            if functions:
                modules[module_name] = sorted(list(functions))
                print(f"  {module_name}: {len(functions)} functions")
    
    return modules

def main():
    """Main entry point."""
    print("Generating win16-api-status.json...")
    print()
    
    # Extract functions from source files
    modules = extract_win16_functions()
    
    if not modules:
        print("Error: No Win16 modules found")
        return 1
    
    # Create JSON output
    output = {
        "modules": [
            {
                "name": name,
                "functions": funcs
            }
            for name, funcs in sorted(modules.items())
        ]
    }
    
    # Write to output file
    output_path = Path("Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/win16-api-status.json")
    output_path.parent.mkdir(parents=True, exist_ok=True)
    
    with open(output_path, 'w') as f:
        json.dump(output, f, indent=2)
    
    print()
    print(f"Successfully generated {output_path}")
    print(f"Total modules: {len(modules)}")
    print(f"Total functions: {sum(len(funcs) for funcs in modules.values())}")
    
    return 0

if __name__ == "__main__":
    import sys
    sys.exit(main())
