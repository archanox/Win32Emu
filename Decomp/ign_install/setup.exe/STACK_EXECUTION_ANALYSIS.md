# Stack Execution Analysis - NULL Jump Root Cause

## Discovery

After adding instruction history tracking, we discovered that the NULL jump occurs **after executing ~50 instructions from stack memory**.

## Evidence from Logs

```
Step 1043272: EIP=0x001FE64C    <- Executing from stack
Step 1043273: EIP=0x001FE64E
...
Step 1043321: EIP=0x001FE6AD    <- Last instruction before NULL

ESP=0x001FFF44                   <- Stack pointer
Stack: 0x001FE6AF 0x00402346 0x00400000 0x0040A55C 0x00000000 0x00401130 0x00000000 0x00000000
```

### Key Observations

1. **EIP addresses (0x001FE64C - 0x001FE6AD) are in stack region**
   - ESP = 0x001FFF44 (stack pointer)
   - EIP is ~0x300 bytes below ESP
   - This is definitely stack memory, not code segment

2. **Pattern of execution**
   - Exactly 50 instructions (1043272-1043321)
   - All sequential, 2 bytes apart (0x001FE64C, 0x001FE64E, etc.)
   - Suggests small instructions (likely single-byte or two-byte opcodes)

3. **Stack contents show NULL return address**
   ```
   [ESP+0]  = 0x001FE6AF  <- Next address after last instruction
   [ESP+4]  = 0x00402346  <- Valid return address  
   [ESP+8]  = 0x00400000  <- Module base
   [ESP+12] = 0x0040A55C  <- Parameter (likely dialog template)
   [ESP+16] = 0x00000000  <- NULL! This is what got jumped to
   [ESP+20] = 0x00401130  <- Dialog procedure address
   ```

## Root Cause Analysis

### Why is code executing from the stack?

**Possibility 1: Dynamic Code Generation**
- Some compilers generate small thunks/trampolines on the stack
- Used for callbacks, closures, or nested functions
- This code sets up parameters and jumps to the real function

**Possibility 2: Buffer Overflow/Code Injection**
- Accidental buffer overflow writing code to stack
- Less likely given the clean pattern

**Possibility 3: SEH (Structured Exception Handling)**
- Windows SEH handlers are sometimes placed on stack
- Could be exception unwinding code
- Less likely - would have different pattern

**Possibility 4: Incorrect Function Pointer**
- Something called a function pointer that points to stack
- The "function" at that address isn't actually valid code
- When it tries to return, it pops NULL and crashes

### Why does it have a NULL return address?

The stack setup is:
```
0x001FE6AF <- Return from stack code
0x00402346 <- Valid address
0x00400000 <- Parameters...
0x0040A55C
0x00000000 <- NULL return address
0x00401130 <- Dialog proc
```

This suggests:
1. Something set up a call frame with NULL as a return address
2. The stack code ran 50 instructions
3. It executed a `ret` instruction
4. The `ret` popped 0x00000000 and jumped to it
5. **CRASH**

## Investigation Steps

### Step 1: Find what jumped to stack address

Need to look at the instruction history BEFORE the stack execution starts. The history should show:
- An import call or COM call
- That returned with EIP=0x001FE64C (the first stack address)

### Step 2: Examine the import table

Check if there's a missing or incorrectly implemented API that should return a real function address but is returning a stack address or NULL.

### Step 3: Check for thunk/trampoline patterns

Disassemble the code at 0x001FE64C to see what it's actually doing:
- Is it a `push/call/ret` sequence?
- Is it setting up parameters?
- What is the final instruction that causes the `ret` to NULL?

## Hypothesis

**Most Likely:** An API function is returning a function pointer that points to stack memory (0x001FE64C). This is probably:

1. A callback setup function that's supposed to create a thunk
2. The emulator is incorrectly implementing it
3. Instead of returning a valid function pointer, it's returning a stack address
4. The code jumps there, executes garbage instructions
5. Eventually hits a `ret` that pops the NULL and crashes

**Alternative:** The dialog procedure setup is incorrect and is creating a bad call frame where:
1. Return addresses aren't properly aligned
2. The stack has NULL in the wrong place
3. When unwinding, it hits the NULL

## Solution Direction

### Immediate Fix
Add stack execution detection and abort early with better diagnostics:
- Warn when EIP is in stack region
- Log what called the stack address
- Prevent infinite execution in stack region

### Root Fix
Need to find which API call returns the stack address:
1. Add logging for ALL function returns that could be function pointers
2. Check LoadImageA, LoadStringA, or any callback setup
3. Find where 0x001FE64C comes from
4. Fix that API to return proper value or NULL

## Next Steps

1. ✅ Add stack execution detection (commit pending)
2. Increase history buffer to 100 or track calls before stack execution
3. Add logging of all calls that return addresses
4. Identify which API created the stack pointer
5. Fix that API implementation
