#include "types-and-globals.h"
#include "helpers.h"

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x401000_Code_x86(generic32_t argument_0) {
  struct_331 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  var_5 = argument_0;
  stack.offset_8 = argument_0;
  var_4 = ((cabifunction_717 *) segment_3.offset_688)();
  var_3 = !var_4 ? 64 : 0;
  var_2 = lshift(var_4, 4294967272);
  var_6 = &stack.offset_8;
  if (!(var_3 | (var_2 & 0x80))) {
    var_6 = &stack.offset_8;
    switch ((number8_t) *(generic8_t *) (var_4 + argument_0 - 1)) {
      case 47:
      case 92:
      {
        *(generic32_t *) (var_6 - 4) = ((struct_331 *) var_6)->offset_16;
        *(generic32_t *) (var_6 - 8) = argument_0;
        var_1 = ((cabifunction_719 *) segment_3.offset_712)();
        *(generic32_t *) (var_6 - 12) = argument_0;
        var_0 = ((cabifunction_720 *) segment_3.offset_688)();
        *(generic8_t *) (var_0 + argument_0 + 1) = '\000';
        revng_abort("A longjmp was taken");
      } break;
      default:
      {
        stack.offset_4 = (pointer_or_number32_t) &segment_2 + 9452;
        stack.offset_0 = argument_0;
        ((cabifunction_718 *) segment_3.offset_712)();
        var_6 = &stack;
        *(generic32_t *) (var_6 - 4) = ((struct_331 *) var_6)->offset_16;
        *(generic32_t *) (var_6 - 8) = argument_0;
        var_1 = ((cabifunction_719 *) segment_3.offset_712)();
        *(generic32_t *) (var_6 - 12) = argument_0;
        var_0 = ((cabifunction_720 *) segment_3.offset_688)();
        *(generic8_t *) (var_0 + argument_0 + 1) = '\000';
        revng_abort("A longjmp was taken");
      } break;
    }
  } else {
    *(generic32_t *) (var_6 - 4) = ((struct_331 *) var_6)->offset_16;
    *(generic32_t *) (var_6 - 8) = argument_0;
    var_1 = ((cabifunction_719 *) segment_3.offset_712)();
    *(generic32_t *) (var_6 - 12) = argument_0;
    var_0 = ((cabifunction_720 *) segment_3.offset_688)();
    *(generic8_t *) (var_0 + argument_0 + 1) = '\000';
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x401050_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2) {
  struct_332 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  var_1 = argument_0;
  var_2 = argument_1;
  var_3 = argument_2;
  stack.offset_48 = argument_1;
  stack.offset_44 = argument_0;
  var_0 = ((cabifunction_721 *) segment_3.offset_804)();
  if (!var_0) {
    revng_abort("A longjmp was taken");
  } else {
    generic32_t var_4;
    stack.offset_40 = 12320;
    stack.offset_36 = var_3;
    stack.offset_32 = var_2;
    stack.offset_28 = 0;
    stack.offset_24 = var_1;
    stack.offset_20 = segment_2.offset_14864;
    var_4 = ((cabifunction_722 *) segment_3.offset_800)();
    if (!var_4) {
      revng_abort("A longjmp was taken");
    } else {
      generic32_t var_5;
      stack.offset_16 = var_4;
      stack.offset_12 = 0;
      stack.offset_8 = 370;
      stack.offset_4 = var_0;
      var_5 = ((cabifunction_723 *) segment_3.offset_796)();
      if (!var_5) {
        revng_abort("A longjmp was taken");
      } else {
        stack.offset_0 = var_5;
        ((cabifunction_724 *) segment_3.offset_560)();
        revng_abort("A longjmp was taken");
      }
    }
  }
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4010b0_Code_x86(struct_292 argument_0) {
  struct_333 stack;
  generic32_t var_0;
  stack.offset_20 = &(&stack)[1].offset_12;
  stack.offset_16 = argument_0.offset_4;
  stack.offset_12 = (pointer_or_number32_t) &stack.offset_20 + 4;
  ((cabifunction_725 *) segment_3.offset_812)();
  var_0 = stack.offset_144;
  stack.offset_8 = &stack.offset_12;
  stack.offset_4 = 1009;
  stack.offset_0 = var_0;
  ((cabifunction_726 *) segment_3.offset_808)();
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4010f0_Code_x86(void) {
  struct_334 stack;
  struct_455 **var_0;
  stack.offset_12 = (pointer_or_number32_t) &stack.offset_12 + 4;
  ((cabifunction_727 *) segment_3.offset_768)();
  var_0 = stack.offset_12;
  if (!var_0) {
    revng_abort("A longjmp was taken");
  } else {
    stack.offset_8 = (&stack)[1].offset_0;
    stack.offset_4 = var_0;
    ((cabifunction_728 *) (*var_0)->offset_20)();
    stack.offset_0 = stack.offset_4;
    ((cabifunction_729 *) (*stack.offset_4)->offset_8)();
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x401130_Code_x86(void) {
  function_0x402820_Code_x86();
}

_ABI(Microsoft_x86_cdecl)
void function_0x4021ac_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic8_t var_14;
  helper_boundl_wrapper(NULL, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), function_0x4021ac_Code_x86, 4294967295, 514, 0, 0, 0, 0, 4294967295, &var_3, &var_4, &var_5, &var_6, &var_7, &var_8, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14);
  helper_daa_wrapper(NULL, var_5 + 1 + *(generic8_t *) (var_5 + 1), 10, *(generic8_t *) (var_5 + 1), 0, ((var_5 + 1) & 0xFFFFFF00) | ((var_5 + 1 + *(generic8_t *) (var_5 + 1)) & 0xFF), &var_1, &var_2);
  *(generic32_t *) var_2 = *(generic32_t *) var_2 & var_2;
  var_0 = lshift(*(generic32_t *) var_2 & var_2, 4294967272);
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4021c0_Code_x86(void) {
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4022c0_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2) {
  struct_335 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  var_1 = argument_0;
  var_2 = argument_1;
  var_3 = argument_2;
  stack.offset_12 = argument_2;
  stack.offset_8 = argument_1;
  stack.offset_4 = argument_0;
  var_0 = ((cabifunction_730 *) segment_3.offset_804)();
  stack.offset_0 = var_0;
  ((cabifunction_731 *) segment_3.offset_852)();
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4022e0_Code_x86(generic32_t argument_0) {
  struct_336 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic64_t var_3;
  var_2 = &stack;
  var_1 = argument_0;
  ((struct_336 *) var_2)->offset_8 = 260;
  ((struct_336 *) var_2)->offset_4 = &segment_2.offset_14872;
  segment_2.offset_14864 = argument_0;
  ((struct_336 *) var_2)->offset_0 = argument_0;
  var_0 = ((rawfunction_111 *) segment_3.offset_572)();
  var_3 = 4241944;
  if (!segment_2.offset_14872) {
    *(generic32_t *) (var_2 - 4) = 0;
    *(generic8_t *) var_3 = '\000';
    ((cabifunction_732 *) segment_3.offset_880)();
    *(generic32_t *) (var_2 - 8) = 0;
    *(cabifunction_806 **) (var_2 - 12) = function_0x401130_Code_x86;
    *(generic32_t *) (var_2 - 16) = 0;
    *(generic32_t *) (var_2 - 20) = (pointer_or_number32_t) &segment_2 + 9564;
    *(generic32_t *) (var_2 - 24) = argument_0;
    ((cabifunction_733 *) segment_3.offset_820)();
    ((cabifunction_734 *) segment_3.offset_876)();
    revng_abort("A longjmp was taken");
  } else {
    generic32_t var_4;
    generic32_t var_5;
    generic32_t var_6;
    var_5 = &segment_2.offset_14872;
    var_6 = &segment_2.offset_14872;
    var_4 = 0;
    generic32_t var_7;
    generic32_t var_8;
    generic32_t var_9;
    generic32_t var_10;
    artificial_struct_returned_by_rawfunction_112 var_11;
    do {
      var_7 = (pointer_or_number32_t) &stack - 4 - (var_4 << 2);
      *(generic32_t *) var_7 = var_5;
      var_10 = *(generic8_t *) var_5 == '\\' || *(generic8_t *) var_5 == '/' ? var_5 : var_6;
      var_6 = var_10;
      var_11 = ((rawfunction_112 *) segment_3.offset_816)();
      var_9 = var_11.register_eax;
      var_5 = var_9;
      var_8 = var_11.register_ecx;
      var_4 = var_4 + 1;
    } while (*(generic8_t *) var_5);
    var_3 = var_10;
    var_2 = var_7;
    *(generic32_t *) (var_2 - 4) = 0;
    *(generic8_t *) var_3 = '\000';
    ((cabifunction_732 *) segment_3.offset_880)();
    *(generic32_t *) (var_2 - 8) = 0;
    *(cabifunction_806 **) (var_2 - 12) = function_0x401130_Code_x86;
    *(generic32_t *) (var_2 - 16) = 0;
    *(generic32_t *) (var_2 - 20) = (pointer_or_number32_t) &segment_2 + 9564;
    *(generic32_t *) (var_2 - 24) = argument_0;
    ((cabifunction_733 *) segment_3.offset_820)();
    ((cabifunction_734 *) segment_3.offset_876)();
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x402360_Code_x86(void) {
  struct_337 stack;
  generic32_t var_0;
  generic32_t var_1;
  stack.offset_92 = (pointer_or_number32_t) &stack.offset_92 + 8;
  stack.offset_88 = (pointer_or_number32_t) &segment_1 + 112;
  stack.offset_84 = 1;
  *(generic32_t *) &stack.offset_80 = 0;
  stack.offset_76 = (pointer_or_number32_t) &segment_1 + 16;
  var_1 = ((cabifunction_735 *) segment_3.offset_884)();
  var_0 = lshift(var_1, 4294967272);
  if (!(var_0 & 0x80)) {
    generic32_t **var_2;
    struct_549 **var_3;
    struct_548 **var_4;
    generic32_t var_5;
    generic32_t var_6;
    generic32_t var_7;
    stack.offset_72 = stack.offset_612;
    stack.offset_68 = *(generic32_t *) &stack.offset_80;
    ((cabifunction_736 *) *(generic32_t *) (*(generic32_t *) *(generic32_t *) &stack.offset_80 + 80))();
    var_4 = stack.offset_72;
    stack.offset_64 = stack.offset_612;
    stack.offset_60 = var_4;
    ((cabifunction_737 *) (*var_4)->offset_28)();
    var_3 = stack.offset_64;
    stack.offset_56 = stack.offset_608;
    stack.offset_52 = var_3;
    ((cabifunction_738 *) (*var_3)->offset_36)();
    var_2 = stack.offset_56;
    stack.offset_48 = &stack.offset_60;
    stack.offset_44 = (pointer_or_number32_t) &segment_1 + 864;
    stack.offset_40 = var_2;
    var_6 = ((cabifunction_739 *) **var_2)();
    var_5 = lshift(var_6, 4294967272);
    var_7 = &stack.offset_40;
    if (!(var_5 & 0x80)) {
      struct_550 **var_8;
      generic32_t var_9;
      generic32_t var_10;
      generic32_t var_11;
      stack.offset_36 = 260;
      var_9 = stack.offset_580;
      stack.offset_32 = &stack.offset_52;
      stack.offset_28 = 4294967295;
      stack.offset_24 = var_9;
      stack.offset_20 = 0;
      stack.offset_16 = 0;
      ((cabifunction_740 *) segment_3.offset_568)();
      stack.offset_12 = 1;
      var_8 = stack.offset_24;
      stack.offset_8 = &stack.offset_28;
      stack.offset_4 = var_8;
      var_11 = ((cabifunction_741 *) (*var_8)->offset_24)();
      var_10 = lshift(var_11, 4294967272);
      if ((var_10 & 0x80)) {
        revng_abort("A longjmp was taken");
      }
      stack.offset_0 = stack.offset_12;
      ((cabifunction_742 *) (*stack.offset_12)->offset_8)();
      var_7 = &stack;
    }
    *(struct_550 ***) (var_7 - 4) = ((struct_337 *) var_7)->offset_4;
    ((cabifunction_743 *) (*((struct_337 *) var_7)->offset_4)->offset_8)();
    revng_abort("A longjmp was taken");
  } else {
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
void function_0x40242c_Code_x86(void) {
}

_ABI(Microsoft_x86_cdecl)
void function_0x402440_Code_x86(void) {
  if (*(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20332 + 4)) {
    ((cabifunction_745 *) *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20332 + 4))();
  }
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = (pointer_or_number32_t) &segment_2 + 16;
  *(generic32_t *) (revng_undefined_local_sp() - 8) = (pointer_or_number32_t) &segment_2 + 8;
  function_0x402560_Code_x86((struct_475 *) var_0, var_1);
  *(generic32_t *) (revng_undefined_local_sp() - 4) = (pointer_or_number32_t) &segment_2 + 4;
  *(struct_2 **) (revng_undefined_local_sp() - 8) = &segment_2;
  function_0x402560_Code_x86((struct_475 *) var_2, var_3);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x402470_Code_x86(generic32_t argument_0) {
  struct_339 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  stack.offset_4 = 0;
  stack.offset_0 = 0;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = argument_0;
  function_0x4024b0_Code_x86(var_1);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x402490_Code_x86(generic32_t argument_0) {
  struct_341 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  stack.offset_4 = 0;
  stack.offset_0 = 1;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = argument_0;
  function_0x4024b0_Code_x86(var_1);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4024b0_Code_x86(generic32_t argument_0) {
  struct_340 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  var_1 = (pointer_or_number32_t) &stack.offset_4 + 4;
  if (segment_2.offset_9664 == 1) {
    generic32_t var_2;
    stack.offset_4 = argument_0;
    var_2 = ((cabifunction_746 *) segment_3.offset_740)();
    stack.offset_0 = var_2;
    ((cabifunction_747 *) segment_3.offset_748)();
    var_1 = &stack;
  }
  generic32_t var_3;
  generic32_t var_4;
  segment_2.offset_9660 = 1;
  segment_2.offset_9656 = (number8_t) ((struct_340 *) var_1)[1].offset_4;
  if (!((struct_340 *) var_1)[1].offset_0) {
    if (segment_2.offset_20332) {
      if (!(segment_2.offset_20328 - 4 < segment_2.offset_20332)) {
        generic32_t var_5;
        generic32_t var_6;
        var_5 = 0;
        var_6 = segment_2.offset_20328 - 4;
        generic8_t var_7;
        do {
          if (*(generic32_t *) var_6) {
            ((cabifunction_748 *) *(generic32_t *) var_6)();
          }
          var_6 = var_6 - 4;
          var_7 = segment_2.offset_20328 - 8 - (var_5 << 2) < segment_2.offset_20332;
          var_5 = var_5 + 1;
        } while (!(var_7));
      }
    }
    var_4 = var_1 - 4;
    *(generic32_t *) var_4 = (pointer_or_number32_t) &segment_2 + 28;
    var_3 = var_1 - 8;
    *(generic32_t *) var_3 = (pointer_or_number32_t) &segment_2 + 20;
    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
    function_0x402560_Code_x86((struct_475 *) NULL, 0);
  } else {
    var_4 = var_1 - 4;
    var_3 = var_1 - 8;
  }
  *(generic32_t *) var_4 = (pointer_or_number32_t) &segment_2 + 36;
  *(generic32_t *) var_3 = (pointer_or_number32_t) &segment_2 + 32;
  revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
  function_0x402560_Code_x86((struct_475 *) NULL, 0);
  if (!((struct_340 *) var_1)[1].offset_4) {
    segment_2.offset_9664 = 1;
    *(generic32_t *) var_4 = argument_0;
    ((cabifunction_749 *) segment_3.offset_744)();
    revng_abort("A longjmp was taken");
  } else {
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
void function_0x402560_Code_x86(struct_475 *argument_0, generic32_t argument_1) {
  struct_475 *var_0;
  generic32_t var_1;
  var_0 = argument_0;
  var_1 = argument_1;
  if (argument_1 > (uint32_t) argument_0) {
    generic32_t var_2;
    generic32_t var_3;
    var_2 = 0;
    var_3 = argument_0;
    generic8_t var_4;
    do {
      if (((struct_475 *) var_3)->offset_0) {
        ((cabifunction_750 *) ((struct_475 *) var_3)->offset_0)();
      }
      var_4 = (pointer_or_number32_t) &argument_0->offset_4 + var_2 * 4 < argument_1;
      var_2 = var_2 + 1;
      var_3 = &((struct_475 *) var_3)->offset_4;
    } while (var_4);
  }
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x402580_Code_x86(generic32_t argument_0, generic32_t argument_1) {
  struct_342 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  struct_295 var_4;
  var_2 = argument_0;
  var_3 = argument_1;
  stack.offset_20 = 66;
  stack.offset_16 = argument_0;
  stack.offset_0 = &(&stack)[1].offset_12;
  stack.offset_8 = argument_0;
  stack.offset_12 = 2147483647;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = var_3;
  *(generic8_t ***) ((pointer_or_number32_t) &stack - 8) = &stack.offset_8;
  var_1 = function_0x402b70_Code_x86(var_4);
  var_0 = stack.offset_12;
  stack.offset_12 = var_0 - 1;
  if ((int32_t) var_0 < (int32_t) 1 && (int32_t) var_0 > -2147483648) {
    generic32_t var_5;
    generic32_t var_6;
    union_588 var_7;
    stack.offset_0 = &stack.offset_8;
    *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 0;
    var_5 = function_0x402a20_Code_x86(var_7, (struct_552 *) var_6);
  } else {
    *stack.offset_8 = '\000';
    stack.offset_8 = &stack.offset_8[1];
  }
  return var_1;
}

_ABI(Microsoft_x86_cdecl)
struct_635 function_0x4025f0_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2, generic32_t argument_3) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  var_3 = argument_0;
  var_4 = argument_1;
  var_5 = argument_2;
  var_6 = argument_3;
  var_0 = lshift(argument_1, 4294967272);
  var_7 = 0;
  if ((var_0 & 0x80)) {
    var_4 = (pointer_or_number32_t) (var_3 != 0) - argument_1;
    var_3 = 0 - var_3;
    var_7 = 1;
  }
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  var_10 = var_7;
  var_9 = var_6;
  var_8 = lshift(var_9, 4294967272);
  if ((var_8 & 0x80)) {
    var_10 = var_7 + 1;
    var_9 = (pointer_or_number32_t) (var_5 != 0) - var_6;
    var_6 = var_9;
    var_5 = 0 - var_5;
  }
  generic32_t var_11;
  generic32_t var_12;
  if (!var_9) {
    var_12 = var_4 / *(generic32_t *) (revng_undefined_local_sp() + 12);
    var_11 = (number32_t) ((((number64_t) (var_4 % *(generic32_t *) (revng_undefined_local_sp() + 12)) << 32) | var_3) / *(generic32_t *) (revng_undefined_local_sp() + 12));
  } else {
    generic32_t var_13;
    generic32_t var_14;
    generic32_t var_15;
    generic32_t var_16;
    var_13 = var_3;
    var_14 = var_9;
    var_15 = *(generic32_t *) (revng_undefined_local_sp() + 12);
    var_16 = var_4;
    generic32_t var_17;
    generic32_t var_18;
    generic32_t var_19;
    generic32_t var_20;
    generic32_t var_21;
    generic32_t var_22;
    generic32_t var_23;
    generic32_t var_24;
    generic32_t var_25;
    generic32_t var_26;
    do {
      var_18 = var_14;
      var_17 = var_16;
      var_14 = var_18 >> 1;
      var_26 = var_18 < 2 ? 64 : 0;
      var_25 = lshift(var_14, 4294967272);
      var_24 = lshift(var_18 ^ var_14, 4294967276);
      var_23 = helper_rcrl_wrapper(NULL, var_15, 1, (((llvm_ctpop_i32(var_14 & 0xFF) << 2) & 0x4) | (var_18 & 0x1) | var_26 | (var_25 & 0x80) | (var_24 & 0x800)) ^ 0x4, &var_2);
      var_15 = var_23;
      var_16 = var_17 >> 1;
      var_22 = var_17 < 2 ? 64 : 0;
      var_21 = lshift(var_16, 4294967272);
      var_20 = lshift(var_17 ^ var_16, 4294967276);
      var_19 = helper_rcrl_wrapper(NULL, var_13, 1, (((llvm_ctpop_i32(var_16 & 0xFF) << 2) & 0x4) | (var_17 & 0x1) | var_22 | (var_21 & 0x80) | (var_20 & 0x800)) ^ 0x4, &var_1);
      var_13 = var_19;
    } while (!(var_18 < 2));
    if (var_6 * (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23) + (number32_t) ((uint64_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23 * *(generic32_t *) (revng_undefined_local_sp() + 12)) >> 32) < var_6 * (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23)) {
      var_11 = (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23) - 1;
      var_12 = 0;
    } else {
      if (var_6 * (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23) + (number32_t) ((uint64_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23 * *(generic32_t *) (revng_undefined_local_sp() + 12)) >> 32) > var_4) {
        var_11 = (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23) - 1;
        var_12 = 0;
      } else {
        var_11 = (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23);
        var_12 = 0;
        if (var_6 * (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23) + (number32_t) ((uint64_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23 * *(generic32_t *) (revng_undefined_local_sp() + 12)) >> 32) - var_4 <= ~var_4 && var_3 < (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23 * *(generic32_t *) (revng_undefined_local_sp() + 12))) {
          var_11 = (number32_t) ((((number64_t) (var_17 >> 1) << 32) | var_19) / var_23) - 1;
          var_12 = 0;
        }
      }
    }
  }
  generic32_t var_27;
  generic32_t var_28;
  struct_635 var_29;
  var_28 = var_10 == 1 ? 0 - var_11 : var_11;
  var_27 = var_10 == 1 ? (pointer_or_number32_t) (var_11 != 0) - var_12 : var_12;
  var_29.offset_0 = var_28;
  var_29.offset_4 = var_27;
  return var_29;
}

_ABI(Microsoft_x86_cdecl)
struct_637 function_0x4026a0_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2, generic32_t argument_3) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic64_t var_4;
  generic32_t var_5;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  var_3 = argument_3;
  if (!(argument_3 | argument_1)) {
    var_4 = argument_2 * var_0;
    var_5 = (number32_t) ((uint64_t) var_4 >> 32);
  } else {
    var_4 = argument_2 * var_0;
    var_5 = argument_2 * argument_1 + var_0 * argument_3 + (number32_t) ((uint64_t) var_4 >> 32);
  }
  struct_637 var_6;
  var_6.offset_0 = (number32_t) var_4;
  var_6.offset_4 = var_5;
  return var_6;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4026e0_Code_x86(struct_478 *argument_0) {
  struct_344 stack;
  struct_478 *var_0;
  struct_478 *var_1;
  var_0 = argument_0;
  var_1 = argument_0;
  struct_478 *var_2;
  generic32_t var_3;
  do {
    var_2 = var_1;
    if ((int32_t) segment_2.offset_10236 > (int32_t) 1) {
      struct_654 var_4;
      generic32_t var_5;
      generic32_t var_6;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 8;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = var_2->offset_0;
      var_4 = function_0x403630_Code_x86(var_5, var_6);
      var_3 = var_4.offset_0;
    } else {
      var_3 = *(generic16_t *) ((var_2->offset_0 << 1) + segment_2.offset_9712) & 0x8;
    }
    var_1 = &var_2[1];
  } while (var_3);
  generic8_t var_7;
  struct_478 *var_8;
  var_7 = var_2->offset_0;
  var_8 = &var_2[1];
  switch ((number8_t) var_7) {
    case 43:
    case 45:
    {
      var_7 = var_2[1].offset_0;
      var_8 = &var_2[2];
    } break;
  }
  generic32_t var_9;
  generic8_t var_10;
  generic32_t var_11;
  var_10 = var_7;
  var_11 = var_8;
  var_9 = 0;
  while (true) {
    generic32_t var_12;
    generic32_t var_13;
    var_12 = var_11;
    if ((int32_t) segment_2.offset_10236 > (int32_t) 1) {
      struct_654 var_14;
      generic32_t var_15;
      generic32_t var_16;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 4;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = var_10;
      var_14 = function_0x403630_Code_x86(var_15, var_16);
      var_13 = var_14.offset_0;
    } else {
      var_13 = *(generic16_t *) ((var_10 << 1) + segment_2.offset_9712) & 0x4;
    }
    if (!var_13) {
      break;
    }
    var_11 = var_12 + 1;
    var_9 = var_9 * 10 + var_10 - 48;
    var_10 = *(generic8_t *) var_12;
  }
  generic32_t var_17;
  var_17 = var_2->offset_0 == '-' ? 0 - var_9 : var_9;
  return var_17;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x402790_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_1 = argument_0;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = argument_0;
  var_0 = function_0x4026e0_Code_x86((struct_478 *) var_2);
  return var_0;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4027a0_Code_x86(struct_480 *argument_0, struct_471 *argument_1) {
  uint64_t loop_state_var;
  struct_480 *var_0;
  struct_471 *var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = var_0;
  if (argument_1->offset_0) {
    if (!argument_1->offset_1) {
      generic32_t var_3;
      *(generic32_t *) (revng_undefined_local_sp() - 4) = *(generic32_t *) (revng_undefined_local_sp() - 8);
      var_3 = var_0;
      if ((var_3 & 0x3)) {
        generic32_t var_4;
        generic32_t var_5;
        generic32_t var_6;
        var_4 = 0;
        var_5 = argument_1;
        var_6 = var_0;
        while (true) {
          generic32_t var_7;
          generic32_t var_8;
          generic32_t var_9;
          generic32_t var_10;
          var_10 = var_6;
          var_9 = (pointer_or_number32_t) var_0 + 1 + var_4;
          var_8 = (var_5 & 0xFFFFFF00) | *(generic8_t *) var_10;
          var_7 = var_10 + 1;
          if (*(generic8_t *) var_10 != argument_1->offset_0) {
            generic8_t var_11;
            var_11 = !*(generic8_t *) var_10;
            var_10 = 0;
            if (!(var_11)) {
              var_4 = var_4 + 1;
              var_5 = var_8;
              var_6 = var_7;
              if ((var_9 & 0x3)) {
                continue;
              }
              var_3 = var_9;
              break;
            }
          }
          var_2 = var_10;
          return var_2;
        }
      }
      generic32_t var_12;
      var_12 = var_3;
      *(generic32_t *) (revng_undefined_local_sp() - 8) = *(generic32_t *) (revng_undefined_local_sp() - 4);
      *(generic32_t *) (revng_undefined_local_sp() - 12) = *(generic32_t *) (revng_undefined_local_sp() - 12);
      while (true) {
        generic32_t var_13;
        if (!(((2164326656 - (*(generic32_t *) var_12 ^ (((number32_t) (((number32_t) argument_1->offset_0 << 8) | argument_1->offset_0) << 16) | (((number32_t) argument_1->offset_0 << 8) | argument_1->offset_0)))) ^ (*(generic32_t *) var_12 ^ (((number32_t) (((number32_t) argument_1->offset_0 << 8) | argument_1->offset_0) << 16) | (((number32_t) argument_1->offset_0 << 8) | argument_1->offset_0)))) & 0x81010100)) {
          if (!(((*(generic32_t *) var_12 ^ (*(generic32_t *) var_12 + 2130640639)) & 0x81010100) ^ 0x81010100)) {
            var_12 = var_12 + 4;
            continue;
          }
          var_13 = 0;
          if (!(((((*(generic32_t *) var_12 ^ (*(generic32_t *) var_12 + 2130640639)) & 0x81010100) ^ 0x81010100) & 0x1010100) != 0 || (int32_t) *(generic32_t *) var_12 > -2130640640 && (int32_t) *(generic32_t *) var_12 < (int32_t) 16843009)) {
            var_12 = var_12 + 4;
            continue;
          }
        } else {
          var_13 = var_12;
          if (argument_1->offset_0 != (number8_t) *(generic32_t *) var_12) {
            var_13 = 0;
            if ((number8_t) *(generic32_t *) var_12) {
              if (argument_1->offset_0 == (number8_t) ((uint32_t) *(generic32_t *) var_12 >> 8)) {
                var_2 = var_12 + 1;
                break;
              }
              var_13 = 0;
              if ((number8_t) ((uint32_t) *(generic32_t *) var_12 >> 8)) {
                if (argument_1->offset_0 == (number8_t) ((uint32_t) *(generic32_t *) var_12 >> 16)) {
                  var_2 = var_12 + 2;
                  break;
                }
                var_13 = 0;
                if ((*(generic32_t *) var_12 & 0xFF0000)) {
                  if (argument_1->offset_0 == (number8_t) ((uint32_t) *(generic32_t *) var_12 >> 24)) {
                    var_2 = var_12 + 3;
                    break;
                  }
                  var_13 = 0;
                  if (!(*(generic32_t *) var_12 < 16777216)) {
                    var_12 = var_12 + 4;
                    continue;
                  }
                }
              }
            }
          }
        }
        var_2 = var_13;
        break;
      }
    } else {
      generic32_t var_14;
      var_14 = var_0;
      loop_state_var = 1;
      while (true) {
        generic8_t var_15;
        generic32_t var_16;
        generic32_t var_17;
        generic32_t var_18;
        generic32_t var_19;
        generic32_t var_20;
        generic32_t var_21;
        generic32_t var_22;
        generic32_t var_23;
        generic32_t var_24;
        generic32_t var_25;
        if (!(loop_state_var)) {
          var_22 = (var_24 & 0xFFFFFF00) | *(generic8_t *) var_25;
          var_23 = var_25 + 1;
          if (*(generic8_t *) var_25 == argument_1->offset_1) {
            var_17 = 0;
            var_18 = (var_24 & 0xFFFFFF00) | *(generic8_t *) var_25;
            var_19 = var_25 + 1;
            while (true) {
              if (*(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_2 + var_17 * 2)) {
                if (*(generic8_t *) var_19 != *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_2 + var_17 * 2)) {
                  var_16 = ((number32_t) *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_2 + var_17 * 2) << 8) | *(generic8_t *) var_19 | (var_18 & 0xFFFF0000);
                  loop_state_var = 0;
                  break;
                }
                if (*(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_3 + var_17 * 2)) {
                  var_18 = ((number32_t) *(generic8_t *) (var_25 + 2 + (var_17 << 1)) << 8) | *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_3 + var_17 * 2) | (var_18 & 0xFFFF0000);
                  var_15 = *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_3 + var_17 * 2) == *(generic8_t *) (var_25 + 2 + (var_17 << 1));
                  var_17 = var_17 + 1;
                  var_19 = var_19 + 2;
                  if (var_15) {
                    continue;
                  }
                  var_16 = var_18;
                  loop_state_var = 1;
                  break;
                }
              }
              var_2 = var_25 - 1;
              loop_state_var = 2;
              break;
            }
            if (loop_state_var == 2) {
              break;
            }
            loop_state_var = 1;
            continue;
          }
        } else {
          generic32_t var_26;
          var_24 = (var_26 & 0xFFFFFF00) | *(generic8_t *) var_14;
          var_25 = var_14 + 1;
          if (*(generic8_t *) var_14 != argument_1->offset_0) {
            var_20 = (var_26 & 0xFFFFFF00) | *(generic8_t *) var_14;
            var_21 = var_14 + 1;
            if (!*(generic8_t *) var_14) {
              var_2 = 0;
              break;
            }
            var_22 = (var_20 & 0xFFFFFF00) | *(generic8_t *) var_21;
            var_23 = var_21 + 1;
            loop_state_var = 2;
            continue;
          }
          var_22 = (var_24 & 0xFFFFFF00) | *(generic8_t *) var_25;
          var_23 = var_25 + 1;
          if (*(generic8_t *) var_25 == argument_1->offset_1) {
            var_17 = 0;
            var_18 = (var_24 & 0xFFFFFF00) | *(generic8_t *) var_25;
            var_19 = var_25 + 1;
            while (true) {
              if (*(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_2 + var_17 * 2)) {
                if (*(generic8_t *) var_19 != *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_2 + var_17 * 2)) {
                  var_16 = ((number32_t) *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_2 + var_17 * 2) << 8) | *(generic8_t *) var_19 | (var_18 & 0xFFFF0000);
                  loop_state_var = 0;
                  break;
                }
                if (*(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_3 + var_17 * 2)) {
                  var_18 = ((number32_t) *(generic8_t *) (var_25 + 2 + (var_17 << 1)) << 8) | *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_3 + var_17 * 2) | (var_18 & 0xFFFF0000);
                  var_15 = *(generic8_t *) ((pointer_or_number32_t) &argument_1->offset_3 + var_17 * 2) == *(generic8_t *) (var_25 + 2 + (var_17 << 1));
                  var_17 = var_17 + 1;
                  var_19 = var_19 + 2;
                  if (var_15) {
                    continue;
                  }
                  var_16 = var_18;
                  loop_state_var = 1;
                  break;
                }
              }
              var_2 = var_25 - 1;
              loop_state_var = 2;
              break;
            }
            if (loop_state_var == 2) {
              break;
            }
            loop_state_var = 1;
            continue;
          }
        }
        if (argument_1->offset_0 == (number8_t) var_22) {
          loop_state_var = 0;
          continue;
        }
        var_20 = var_22;
        var_21 = var_23;
        if (!(number8_t) var_22) {
          var_2 = 0;
          break;
        }
        var_22 = (var_20 & 0xFFFFFF00) | *(generic8_t *) var_21;
        var_23 = var_21 + 1;
        loop_state_var = 2;
      }
    }
  }
  return var_2;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x402820_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  do {
    var_0 = var_1;
    var_1 = var_0 - 4096;
  } while ((var_0 & 0xFFFFF000) != 4096);
  *(generic32_t *) NULL = *(generic32_t *) revng_undefined_local_sp();
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x402850_Code_x86(void) {
  struct_346 stack;
  generic32_t var_0;
  stack.offset_120 = 4294967295;
  stack.offset_116 = (pointer_or_number32_t) &segment_1 + 1104;
  stack.offset_112 = function_0x404474_Code_x86;
  stack.offset_108 = *(generic32_t *) NULL;
  *(generic32_t **) NULL = &stack.offset_108;
  stack.offset_100 = &stack;
  var_0 = ((cabifunction_751 *) segment_3.offset_716)();
  segment_2.offset_9604.member_1 = var_0;
  segment_2.offset_9616 = segment_2.offset_9604.member_0.offset_1;
  segment_2.offset_9604.member_1 = (uint32_t) segment_2.offset_9604.member_1 >> 16;
  segment_2.offset_9612 = segment_2.offset_9604.member_1 & 0xFF;
  segment_2.offset_9608 = ((number32_t) (segment_2.offset_9604.member_1 & 0xFF) << 8) + segment_2.offset_9616;
  function_0x404430_Code_x86();
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4029a3_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  *(generic32_t *) NULL = *(generic32_t *) *(generic32_t *) *(generic32_t *) NULL;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = *(generic32_t *) *(generic32_t *) *(generic32_t *) NULL;
  *(generic32_t *) (revng_undefined_local_sp() - 8) = *(generic32_t *) NULL;
  var_0 = function_0x403890_Code_x86(var_1, var_2);
  return var_0;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4029be_Code_x86(void) {
  *(generic32_t *) (*(generic32_t *) NULL - 4) = *(generic32_t *) NULL;
  revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
  function_0x402490_Code_x86(0);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4029f0_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  if (segment_2.offset_9692 != 1) {
    *(generic32_t *) (revng_undefined_local_sp() - 4) = var_0;
    function_0x404590_Code_x86(var_1);
  }
  function_0x404550_Code_x86();
  *(generic32_t *) (revng_undefined_local_sp() - 4) = var_0;
  function_0x404590_Code_x86(var_1);
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x402a20_Code_x86(union_588 argument_0, struct_552 *argument_1) {
  struct_343 stack;
  struct_552 *var_0;
  generic32_t var_1;
  var_0 = argument_1;
  if ((argument_1->offset_12 & 0x82) != 0 && !((number8_t) argument_1->offset_12 & 0x40)) {
    generic32_t var_2;
    var_2 = argument_1->offset_12;
    if (((number8_t) argument_1->offset_12 & 0x1)) {
      argument_1->offset_4 = 0;
      if (!(argument_1->offset_12 & 0x10)) {
        argument_1->offset_12 = argument_1->offset_12 | 0x20;
        var_1 = 4294967295;
        return var_1;
      }
      argument_1->offset_0 = argument_1->offset_8;
      var_2 = argument_1->offset_12 & 0xFFFFFFFE;
    }
    argument_1->offset_12 = (var_2 & 0xFFFFFFED) | 0x2;
    argument_1->offset_4 = 0;
    if (!(argument_1->offset_12 & 0x10C)) {
      generic32_t var_3;
      generic32_t var_4;
      switch ((number32_t) argument_1) {
        case 4238232:
        case 4238264:
        {
          generic32_t var_5;
          generic32_t var_6;
          var_4 = (pointer_or_number32_t) &stack - 4;
          *(generic32_t *) var_4 = argument_1->offset_16;
          var_5 = function_0x404ad0_Code_x86(var_6);
          if (!var_5) {
            *(struct_552 **) var_4 = argument_1;
            function_0x404a80_Code_x86((struct_461 *) var_3);
          }
        } break;
        default:
        {
          var_4 = (pointer_or_number32_t) &stack - 4;
          *(struct_552 **) var_4 = argument_1;
          function_0x404a80_Code_x86((struct_461 *) var_3);
        } break;
      }
    }
    generic32_t var_7;
    generic32_t var_8;
    if (!(argument_1->offset_12 & 0x108)) {
      generic32_t var_9;
      generic32_t var_10;
      generic32_t var_11;
      generic32_t var_12;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 1;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = (pointer_or_number32_t) &(&stack)[1] + 4;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = argument_1->offset_16;
      var_9 = function_0x404790_Code_x86(var_10, (generic8_t *) var_11, var_12);
      var_7 = var_9;
      var_8 = 1;
    } else {
      generic32_t var_13;
      generic32_t var_14;
      generic32_t var_15;
      argument_1->offset_0 = &argument_1->offset_8[1];
      argument_1->offset_4 = argument_1->offset_24 - 1;
      var_14 = !(argument_1->offset_0 - (number32_t) argument_1->offset_8) ? 64 : 0;
      var_13 = lshift(argument_1->offset_0 - (number32_t) argument_1->offset_8, 4294967272);
      if (!(var_14 | (var_13 & 0x80))) {
        generic32_t var_16;
        generic32_t var_17;
        generic32_t var_18;
        generic32_t var_19;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = argument_1->offset_0 - (number32_t) argument_1->offset_8;
        *(generic8_t **) ((pointer_or_number32_t) &stack - 8) = argument_1->offset_8;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = argument_1->offset_16;
        var_16 = function_0x404790_Code_x86(var_17, (generic8_t *) var_18, var_19);
        var_15 = var_16;
      } else {
        generic64_t var_20;
        var_20 = 4238036;
        if (argument_1->offset_16 != (pointer_or_number32_t) -1) {
          var_20 = ((argument_1->offset_16 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_1->offset_16 >> 3) & 0xFFFFFFFC) * 1) + 4;
        }
        var_15 = 0;
        if ((*(generic8_t *) var_20 & 0x20)) {
          generic32_t var_21;
          generic32_t var_22;
          generic32_t var_23;
          generic32_t var_24;
          *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 2;
          *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = 0;
          *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = argument_1->offset_16;
          var_21 = function_0x4049c0_Code_x86(var_22, var_23, var_24);
          var_15 = 0;
        }
      }
      var_7 = var_15;
      *argument_1->offset_8 = argument_0.member_0;
      var_8 = argument_1->offset_0 - (number32_t) argument_1->offset_8;
    }
    if (var_7 == var_8) {
      var_1 = argument_0.member_1 & 0xFF;
    } else {
      argument_1->offset_12 = argument_1->offset_12 | 0x20;
      var_1 = 4294967295;
    }
  } else {
    argument_1->offset_12 = argument_1->offset_12 | 0x20;
    var_1 = 4294967295;
  }
  return var_1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x402b70_Code_x86(struct_295 argument_0) {
  generic8_t *var_0;
  var_0 = argument_0.offset_4;
  argument_0.offset_4 = &var_0[1];
  *(generic32_t *) (revng_undefined_local_sp() - 564) = 0;
  *(generic32_t *) (revng_undefined_local_sp() - 540) = 0;
  if ((*var_0) && (!((int32_t) *(generic32_t *) (revng_undefined_local_sp() - 564) < (int32_t) 0))) {
    generic32_t var_1;
    var_1 = *var_0;
    while (true) {
      generic32_t var_2;
      var_2 = 0;
      if (!((number8_t) var_1 < ' ' || (number8_t) var_1 > 'x')) {
        var_2 = *(generic8_t *) (((int32_t) ((number32_t) var_1 << 24) >> 24) + ((pointer_or_number32_t) &segment_1 + 1112)) & 0xF;
      }
      *(generic32_t *) (revng_undefined_local_sp() - 540) = (int8_t) *(generic8_t *) ((var_2 << 3) + *(generic32_t *) (revng_undefined_local_sp() - 540) + ((pointer_or_number32_t) &segment_1 + 1144)) >> '\004';
      if (!((int8_t) *(generic8_t *) ((var_2 << 3) + *(generic32_t *) (revng_undefined_local_sp() - 540) + ((pointer_or_number32_t) &segment_1 + 1144)) > -'\001')) {
        generic8_t *var_3;
        var_3 = argument_0.offset_4;
        argument_0.offset_4 = &var_3[1];
        if (*var_3) {
          var_1 = (var_1 & 0xFFFFFF00) | *var_3;
          if (!((int32_t) *(generic32_t *) (revng_undefined_local_sp() - 564) < (int32_t) 0)) {
            continue;
          }
        }
        break;
      }
      revng_abort("A longjmp was taken");
    }
  }
  return *(generic32_t *) (revng_undefined_local_sp() - 564);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403408_Code_x86(void) {
  generic8_t var_0;
  generic8_t var_1;
  generic8_t var_2;
  generic8_t var_3;
  generic8_t var_4;
  generic8_t var_5;
  generic8_t var_6;
  generic8_t var_7;
  generic8_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic32_t var_36;
  generic32_t var_37;
  generic32_t var_38;
  generic32_t var_39;
  generic32_t var_40;
  generic32_t var_41;
  generic32_t var_42;
  generic32_t var_43;
  generic32_t var_44;
  generic8_t var_45;
  generic32_t var_46;
  generic32_t var_47;
  generic32_t var_48;
  generic32_t var_49;
  *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - 's')) + revng_undefined_local_sp()) = *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - 's')) + revng_undefined_local_sp()) + (((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - 's'));
  *(generic8_t *) NULL = *(generic8_t *) NULL + (*(generic8_t *) 67437827 - '\\');
  helper_das_wrapper(NULL, undef(generic32_t), 6, undef(generic32_t), (uint32_t) (*(generic8_t *) 67437827 > 'p'), (((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + 7, &var_48, &var_49);
  helper_das_wrapper(NULL, ((uint32_t) (var_49 + 2) >> 8) & 0xFF, 6, 0, (uint32_t) (*(generic8_t *) 67437827 > 'p'), ((var_49 + 2) & 0xFFFF00FF) | ((number32_t) (((uint32_t) (var_49 + 2) >> 8) & 0xFF) << 8), &var_46, &var_47);
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) ^ (number8_t) (var_47 + 1);
  *(generic32_t *) 16434 = var_47 + 1;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) = *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_8 = ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1)) : ((var_47 + 1) & 0xFF) > *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_7 = var_8 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 : (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8;
  var_6 = var_7 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 : (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7;
  var_5 = var_6 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 : (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6;
  var_4 = var_5 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 : (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_4;
  var_3 = var_4 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_4 : (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_4;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_2 = var_3 ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3 : ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_1 = var_2 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_2 : (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_2;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_2 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_1;
  var_0 = (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) : (*(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145);
  *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) = *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 3) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144, undef(generic32_t), 0, undef(generic32_t), undef(generic32_t), *(generic32_t *) ((((*(generic8_t *) 67437827 - 'q' - 1343158846) & 0xAFF10700) | (*(generic8_t *) 67437827 - ']')) + ((pointer_or_number32_t) &segment_0 + 7669)), undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36, &var_37, &var_38, &var_39, &var_40, &var_41, &var_42, &var_43, &var_44, &var_45);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403428_Code_x86(void) {
  generic8_t var_0;
  generic8_t var_1;
  generic8_t var_2;
  generic8_t var_3;
  generic8_t var_4;
  generic8_t var_5;
  generic8_t var_6;
  generic8_t var_7;
  generic8_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic32_t var_36;
  generic32_t var_37;
  generic32_t var_38;
  generic32_t var_39;
  generic32_t var_40;
  generic32_t var_41;
  generic32_t var_42;
  generic32_t var_43;
  generic32_t var_44;
  generic8_t var_45;
  generic32_t var_46;
  generic32_t var_47;
  generic32_t var_48;
  generic32_t var_49;
  *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\014')) + revng_undefined_local_sp()) = *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\014')) + revng_undefined_local_sp()) + (((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\014'));
  *(generic8_t *) NULL = *(generic8_t *) NULL + (*(generic8_t *) 67437827 + '#');
  helper_das_wrapper(NULL, undef(generic32_t), 6, undef(generic32_t), (uint32_t) (*(generic8_t *) 67437827 > (uint8_t) -'\017'), (((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + 7, &var_48, &var_49);
  helper_das_wrapper(NULL, ((uint32_t) (var_49 + 2) >> 8) & 0xFF, 6, 0, (uint32_t) (*(generic8_t *) 67437827 > (uint8_t) -'\017'), ((var_49 + 2) & 0xFFFF00FF) | ((number32_t) (((uint32_t) (var_49 + 2) >> 8) & 0xFF) << 8), &var_46, &var_47);
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) ^ (number8_t) (var_47 + 1);
  *(generic32_t *) 16434 = var_47 + 1;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) = *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_8 = ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1)) : ((var_47 + 1) & 0xFF) > *(generic8_t *) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_7 = var_8 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 : (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8;
  var_6 = var_7 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 : (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7;
  var_5 = var_6 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 : (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6;
  var_4 = var_5 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 : (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_4;
  var_3 = var_4 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_4 : (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_8 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_7 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_6 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_5 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_4;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_2 = var_3 ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3 : ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_1 = var_2 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_2 : (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_2;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_2 + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_1;
  var_0 = (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145 ? (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) >= *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) : (*(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) & 0xFF) > *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145);
  *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) = *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) + (number8_t) *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)) + var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 3) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), *(generic32_t *) ((((*(generic8_t *) 67437827 + '\016' - 600111678) & 0xDC3B0700) | (*(generic8_t *) 67437827 + '\"')) + ((pointer_or_number32_t) &segment_0 + 7669)), undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36, &var_37, &var_38, &var_39, &var_40, &var_41, &var_42, &var_43, &var_44, &var_45);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403440_Code_x86(void) {
  generic8_t var_0;
  generic8_t var_1;
  generic8_t var_2;
  generic8_t var_3;
  generic8_t var_4;
  generic8_t var_5;
  generic8_t var_6;
  generic8_t var_7;
  generic8_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic32_t var_36;
  generic32_t var_37;
  generic32_t var_38;
  generic32_t var_39;
  generic32_t var_40;
  generic32_t var_41;
  generic32_t var_42;
  generic32_t var_43;
  generic32_t var_44;
  generic8_t var_45;
  generic32_t var_46;
  generic32_t var_47;
  generic32_t var_48;
  generic32_t var_49;
  *(generic32_t *) (revng_undefined_local_sp() - 768540674) = *(generic32_t *) (revng_undefined_local_sp() - 768540674) - 768540674;
  helper_das_wrapper(NULL, undef(generic32_t), 6, undef(generic32_t), (uint32_t) (*(generic8_t *) 67437827 != 0), 3526426395, &var_48, &var_49);
  helper_das_wrapper(NULL, ((uint32_t) (var_49 + 2) >> 8) & 0xFF, 6, 0, (uint32_t) (*(generic8_t *) 67437827 != 0), ((var_49 + 2) & 0xFFFF00FF) | ((number32_t) (((uint32_t) (var_49 + 2) >> 8) & 0xFF) << 8), &var_46, &var_47);
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) ^ (number8_t) (var_47 + 1);
  *(generic32_t *) 16434 = var_47 + 1;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) *(generic32_t *) (generic32_t) 3530632457 = *(generic8_t *) *(generic32_t *) (generic32_t) 3530632457 + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_8 = ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) *(generic32_t *) (generic32_t) 3530632457 + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1)) : ((var_47 + 1) & 0xFF) > *(generic8_t *) *(generic32_t *) (generic32_t) 3530632457 + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_7 = var_8 ? (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 : (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8;
  var_6 = var_7 ? (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 : (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7;
  var_5 = var_6 ? (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_6 : (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_6;
  var_4 = var_5 ? (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_5 : (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_5;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_5 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_4;
  var_3 = var_4 ? (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_5 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_4 : (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_5 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_4;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_2 = var_3 ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3 : ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_1 = var_2 ? (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_2 : (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_2;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_2 + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_1;
  var_0 = (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145 ? (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) >= *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) : (*(generic32_t *) (generic32_t) 3530632457 & 0xFF) > *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145);
  *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) = *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) + (number8_t) *(generic32_t *) (generic32_t) 3530632457 + var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 3) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), *(generic32_t *) (generic32_t) 3530632457, undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36, &var_37, &var_38, &var_39, &var_40, &var_41, &var_42, &var_43, &var_44, &var_45);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403454_Code_x86(void) {
  generic8_t var_0;
  generic8_t var_1;
  generic8_t var_2;
  generic8_t var_3;
  generic8_t var_4;
  generic8_t var_5;
  generic8_t var_6;
  generic8_t var_7;
  generic8_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic32_t var_36;
  generic32_t var_37;
  generic32_t var_38;
  generic32_t var_39;
  generic32_t var_40;
  generic32_t var_41;
  generic32_t var_42;
  generic32_t var_43;
  generic32_t var_44;
  generic8_t var_45;
  generic32_t var_46;
  generic32_t var_47;
  generic32_t var_48;
  generic32_t var_49;
  helper_das_wrapper(NULL, undef(generic32_t), 6, undef(generic32_t), 0, 29, &var_48, &var_49);
  helper_das_wrapper(NULL, ((uint32_t) (var_49 + 2) >> 8) & 0xFF, 6, 0, 0, ((var_49 + 2) & 0xFFFF00FF) | ((number32_t) (((uint32_t) (var_49 + 2) >> 8) & 0xFF) << 8), &var_46, &var_47);
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) ^ (number8_t) (var_47 + 1);
  *(generic32_t *) 16434 = var_47 + 1;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1);
  *(generic8_t *) *(generic32_t *) (generic32_t) 4206091 = *(generic8_t *) *(generic32_t *) (generic32_t) 4206091 + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_8 = ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) *(generic32_t *) (generic32_t) 4206091 + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1)) : ((var_47 + 1) & 0xFF) > *(generic8_t *) *(generic32_t *) (generic32_t) 4206091 + (number8_t) (var_47 + 1) + (((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1));
  var_7 = var_8 ? (*(generic32_t *) (generic32_t) 4206091 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 : (*(generic32_t *) (generic32_t) 4206091 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8;
  var_6 = var_7 ? (*(generic32_t *) (generic32_t) 4206091 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 : (*(generic32_t *) (generic32_t) 4206091 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7;
  var_5 = var_6 ? (*(generic32_t *) (generic32_t) 4206091 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_6 : (*(generic32_t *) (generic32_t) 4206091 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_6;
  var_4 = var_5 ? (*(generic32_t *) (generic32_t) 4206091 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_5 : (*(generic32_t *) (generic32_t) 4206091 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_5;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_5 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_4;
  var_3 = var_4 ? (*(generic32_t *) (generic32_t) 4206091 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_5 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_4 : (*(generic32_t *) (generic32_t) 4206091 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_8 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_7 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_6 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_5 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_4;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_2 = var_3 ? ((var_47 + 1) & 0xFF) >= *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3 : ((var_47 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_47 + 1) + var_3;
  var_1 = var_2 ? (*(generic32_t *) (generic32_t) 4206091 & 0xFF) >= *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_2 : (*(generic32_t *) (generic32_t) 4206091 & 0xFF) > *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_2;
  *(generic8_t *) (var_47 + 1) = *(generic8_t *) (var_47 + 1) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_2 + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_1;
  var_0 = (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145 ? (*(generic32_t *) (generic32_t) 4206091 & 0xFF) >= *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) : (*(generic32_t *) (generic32_t) 4206091 & 0xFF) > *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145);
  *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) = *(generic8_t *) ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + ((((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) > (uint32_t) -269488145) + (number8_t) *(generic32_t *) (generic32_t) 4206091 + var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 3) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, (((var_47 + 1) & 0xFFFFFF00) | ((var_47 + 17) & 0xFF)) + 269488144, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), *(generic32_t *) (generic32_t) 4206091, undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36, &var_37, &var_38, &var_39, &var_40, &var_41, &var_42, &var_43, &var_44, &var_45);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403468_Code_x86(void) {
  generic8_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic32_t var_36;
  generic8_t var_37;
  generic32_t var_38;
  generic32_t var_39;
  generic32_t var_40;
  generic32_t var_41;
  helper_das_wrapper(NULL, undef(generic32_t), 6, undef(generic32_t), 0, 29, &var_40, &var_41);
  helper_das_wrapper(NULL, ((uint32_t) (var_41 + 2) >> 8) & 0xFF, 6, 0, 0, ((var_41 + 2) & 0xFFFF00FF) | ((number32_t) (((uint32_t) (var_41 + 2) >> 8) & 0xFF) << 8), &var_38, &var_39);
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_39 + 1);
  *(generic8_t *) (var_39 + 1) = *(generic8_t *) (var_39 + 1) ^ (number8_t) (var_39 + 1);
  *(generic32_t *) 16434 = var_39 + 1;
  var_0 = ((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1) ? ((var_39 + 1) & 0xFF) >= *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1)) : ((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1));
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1)) + (number8_t) (var_39 + 1) + var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 3) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, (((var_39 + 1) & 0xFFFFFF00) | ((var_39 + 17) & 0xFF)) + 269488144, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_1, &var_2, &var_3, &var_4, &var_5, &var_6, &var_7, &var_8, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36, &var_37);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403479_Code_x86(void) {
  generic8_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic32_t var_36;
  generic8_t var_37;
  generic32_t var_38;
  generic32_t var_39;
  generic32_t var_40;
  generic32_t var_41;
  helper_das_wrapper(NULL, undef(generic32_t), 6, undef(generic32_t), 0, 29, &var_40, &var_41);
  helper_das_wrapper(NULL, ((uint32_t) (var_41 + 2) >> 8) & 0xFF, 6, 0, 0, ((var_41 + 2) & 0xFFFF00FF) | ((number32_t) (((uint32_t) (var_41 + 2) >> 8) & 0xFF) << 8), &var_38, &var_39);
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_39 + 1);
  *(generic8_t *) (var_39 + 1) = *(generic8_t *) (var_39 + 1) ^ (number8_t) (var_39 + 1);
  *(generic32_t *) 16434 = var_39 + 1;
  var_0 = ((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1) ? ((var_39 + 1) & 0xFF) >= *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1)) : ((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1));
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1)) + (number8_t) (var_39 + 1) + var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 3) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, (((var_39 + 1) & 0xFFFFFF00) | ((var_39 + 17) & 0xFF)) + 269488144, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_1, &var_2, &var_3, &var_4, &var_5, &var_6, &var_7, &var_8, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36, &var_37);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403498_Code_x86(void) {
  generic8_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic32_t var_36;
  generic8_t var_37;
  generic32_t var_38;
  generic32_t var_39;
  generic32_t var_40;
  generic32_t var_41;
  helper_das_wrapper(NULL, undef(generic32_t), 6, undef(generic32_t), 0, undef(generic32_t), &var_40, &var_41);
  helper_das_wrapper(NULL, ((uint32_t) (var_41 + 2) >> 8) & 0xFF, 6, 0, 0, ((var_41 + 2) & 0xFFFF00FF) | ((number32_t) (((uint32_t) (var_41 + 2) >> 8) & 0xFF) << 8), &var_38, &var_39);
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_39 + 1);
  *(generic8_t *) (var_39 + 1) = *(generic8_t *) (var_39 + 1) ^ (number8_t) (var_39 + 1);
  *(generic32_t *) 16434 = var_39 + 1;
  var_0 = ((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1) ? ((var_39 + 1) & 0xFF) >= *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1)) : ((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1));
  *(generic8_t *) NULL = *(generic8_t *) NULL + (number8_t) (var_39 + 1) + (number8_t) (var_39 + 1) + (((var_39 + 1) & 0xFF) > *(generic8_t *) NULL + (number8_t) (var_39 + 1)) + (number8_t) (var_39 + 1) + var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, (((var_39 + 1) & 0xFFFFFF00) | ((var_39 + 17) & 0xFF)) + 269488144, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_1, &var_2, &var_3, &var_4, &var_5, &var_6, &var_7, &var_8, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36, &var_37);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4034dc_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic8_t var_36;
  *(generic8_t *) NULL = *(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))))))));
  *(generic8_t *) NULL = *(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))))))) + (number8_t) *(generic32_t *) NULL + (number8_t) *(generic32_t *) NULL + ((*(generic32_t *) NULL & 0xFF) > *(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)))) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0)) && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0) + (*(generic8_t *) NULL != 0 && !(*(generic8_t *) NULL + (*(generic8_t *) NULL != 0))))))))) + (number8_t) *(generic32_t *) NULL);
  *(generic32_t *) (revng_undefined_local_sp() - 4) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), *(generic32_t *) NULL, undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_0, &var_1, &var_2, &var_3, &var_4, &var_5, &var_6, &var_7, &var_8, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4034df_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  generic8_t var_36;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = 43;
  helper_load_seg_wrapper(NULL, 0, 43, 269488144, undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), undef(generic32_t), (pointer_or_number32_t) &segment_0 + 9469, 4294967295, 514, 4194483, 0, 0, 13630208, 0, 13628160, 0, 0, 65535, 1073741824, 71, 2147549185, 0, 0, 0, 4294967295, &var_0, &var_1, &var_2, &var_3, &var_4, &var_5, &var_6, &var_7, &var_8, &var_9, &var_10, &var_11, &var_12, &var_13, &var_14, &var_15, &var_16, &var_17, &var_18, &var_19, &var_20, &var_21, &var_22, &var_23, &var_24, &var_25, &var_26, &var_27, &var_28, &var_29, &var_30, &var_31, &var_32, &var_33, &var_34, &var_35, &var_36);
}

_ABI(Microsoft_x86_cdecl)
void function_0x403520_Code_x86(union_589 argument_0, struct_555 *argument_1, generic32_t *argument_2) {
  struct_555 *var_0;
  generic32_t *var_1;
  generic8_t var_2;
  var_0 = argument_1;
  var_1 = argument_2;
  argument_1->offset_4 = argument_1->offset_4 - 1;
  if ((int32_t) argument_1->offset_4 < (int32_t) 1 && (int32_t) argument_1->offset_4 > -2147483648) {
    generic32_t var_3;
    generic32_t var_4;
    union_588 var_5;
    *(struct_555 **) (revng_undefined_local_sp() - 4) = argument_1;
    *(generic32_t *) (revng_undefined_local_sp() - 8) = argument_0.member_0;
    var_3 = function_0x402a20_Code_x86(var_5, (struct_552 *) var_4);
    var_2 = var_3 == (pointer_or_number32_t) -1;
  } else {
    *argument_1->offset_0 = argument_0.member_1;
    argument_1->offset_0 = &argument_1->offset_0[1];
    var_2 = false;
  }
  generic32_t var_6;
  var_6 = 4294967295;
  if (!(var_2)) {
    var_6 = *var_1 + 1;
  }
  *var_1 = var_6;
}

_ABI(Microsoft_x86_cdecl)
void function_0x403570_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2, generic32_t *argument_3) {
  struct_353 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t *var_3;
  generic32_t var_4;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  var_3 = argument_3;
  var_4 = argument_1;
  while (true) {
    generic32_t var_5;
    generic32_t var_6;
    var_6 = !var_4 ? 64 : 0;
    var_5 = lshift(var_4, 4294967272);
    if (!(var_6 | (var_5 & 0x80))) {
      generic32_t var_7;
      generic32_t var_8;
      union_589 var_9;
      *(generic32_t **) ((pointer_or_number32_t) &stack - 4) = argument_3;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = argument_2;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = argument_0;
      var_4 = var_4 - 1;
      function_0x403520_Code_x86(var_9, (struct_555 *) var_7, (generic32_t *) var_8);
      if (*argument_3 != (pointer_or_number32_t) -1) {
        continue;
      }
    }
    break;
  }
}

_ABI(Microsoft_x86_cdecl)
void function_0x4035b0_Code_x86(struct_500 *argument_0, generic32_t argument_1, generic32_t argument_2, generic32_t *argument_3) {
  struct_354 stack;
  generic32_t var_0;
  generic32_t var_1;
  struct_500 *var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t *var_5;
  var_2 = argument_0;
  var_3 = argument_1;
  var_4 = argument_2;
  var_5 = argument_3;
  var_1 = !argument_1 ? 64 : 0;
  var_0 = lshift(argument_1, 4294967272);
  if (!(var_1 | (var_0 & 0x80))) {
    generic32_t var_6;
    generic32_t var_7;
    var_6 = 0;
    var_7 = argument_0;
    while (true) {
      generic32_t var_8;
      generic32_t var_9;
      union_589 var_10;
      *(generic32_t **) ((pointer_or_number32_t) &stack - 4) = argument_3;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = argument_2;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = ((struct_500 *) var_7)->offset_0;
      function_0x403520_Code_x86(var_10, (struct_555 *) var_8, (generic32_t *) var_9);
      if (*argument_3 != (pointer_or_number32_t) -1) {
        generic32_t var_11;
        generic32_t var_12;
        var_12 = argument_1 - 1 == var_6 ? 64 : 0;
        var_11 = lshift(argument_1 - 1 - var_6, 4294967272);
        var_6 = var_6 + 1;
        var_7 = &((struct_500 *) var_7)->offset_1;
        if (!(var_12 | (var_11 & 0x80))) {
          continue;
        }
      }
      break;
    }
  }
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4035f0_Code_x86(generic32_t **argument_0) {
  generic32_t **var_0;
  var_0 = argument_0;
  *argument_0 = &(*argument_0)[1];
  return **argument_0;
}

_ABI(Microsoft_x86_cdecl)
struct_651 function_0x403600_Code_x86(struct_568 **argument_0) {
  struct_568 **var_0;
  struct_651 var_1;
  var_0 = argument_0;
  *argument_0 = &(*argument_0)[1];
  var_1.offset_0 = (*argument_0)->offset_0;
  var_1.offset_4 = (*argument_0)->offset_4;
  return var_1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x403620_Code_x86(generic16_t **argument_0) {
  generic16_t **var_0;
  var_0 = argument_0;
  *argument_0 = &(*argument_0)[2];
  return ((number32_t) argument_0 & 0xFFFF0000) | **argument_0;
}

_ABI(Microsoft_x86_cdecl)
struct_654 function_0x403630_Code_x86(generic32_t argument_0, generic32_t argument_1) {
  struct_345 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  var_1 = argument_1;
  if (!(!(argument_0 < (uint32_t) -1 && argument_0 > 255))) {
    generic8_t var_2;
    generic32_t var_3;
    if ((int8_t) *(generic8_t *) ((((uint32_t) argument_0 >> 8 << 1) & 0x1FE) + segment_2.offset_9712 + 1) > -'\001') {
      stack.offset_32 = (number8_t) argument_0;
      var_2 = '\000';
      var_3 = 1;
      stack.offset_33 = var_2;
      stack.offset_20 = 0;
      stack.offset_16 = 0;
      stack.offset_12 = (pointer_or_number32_t) &stack.offset_20 + 10;
      stack.offset_8 = var_3;
      stack.offset_4 = &stack.offset_32;
      stack.offset_0 = 1;
      function_0x404d80_Code_x86();
    }
    stack.offset_32 = (number8_t) ((uint32_t) argument_0 >> 8);
    stack.offset_34 = '\000';
    var_2 = (number8_t) argument_0;
    var_3 = 2;
    stack.offset_33 = var_2;
    stack.offset_20 = 0;
    stack.offset_16 = 0;
    stack.offset_12 = (pointer_or_number32_t) &stack.offset_20 + 10;
    stack.offset_8 = var_3;
    stack.offset_4 = &stack.offset_32;
    stack.offset_0 = 1;
    function_0x404d80_Code_x86();
  }
  struct_654 var_4;
  var_4.offset_0 = argument_1 & *(generic16_t *) ((argument_0 << 1) + segment_2.offset_9712);
  var_4.offset_4 = segment_2.offset_9712;
  return var_4;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x40379c_Code_x86(generic32_t argument_0) {
  struct_356 stack;
  generic32_t var_0;
  var_0 = argument_0;
  stack.offset_16 = (pointer_or_number32_t) &stack.offset_16 + 16;
  stack.offset_12 = 0;
  stack.offset_8 = 0;
  stack.offset_4 = (pointer_or_number32_t) &segment_0 + 10164;
  stack.offset_0 = var_0;
  function_0x406000_Code_x86();
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4037bc_Code_x86(struct_562 *argument_0, generic32_t argument_1, struct_820 argument_2) {
  struct_562 *var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = 1;
  if ((argument_0->offset_4 & 0x6)) {
    *argument_2.offset_4 = argument_1;
    var_2 = 3;
  }
  return var_2;
}

_ABI(Microsoft_x86_cdecl)
void function_0x4037de_Code_x86(struct_502 *argument_0, generic32_t argument_1) {
  struct_357 stack;
  struct_502 *var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  stack.offset_16 = argument_0;
  stack.offset_12 = 4294967294;
  stack.offset_8 = function_0x4037bc_Code_x86;
  stack.offset_4 = *(generic32_t *) NULL;
  *(generic32_t **) NULL = &stack.offset_4;
  var_2 = &stack.offset_4;
  if (*(generic32_t *) ((&stack)[1].offset_4 + 12) == (pointer_or_number32_t) -1) {
    *(generic32_t *) NULL = *(generic32_t *) var_2;
    revng_abort("A longjmp was taken");
  } else {
    generic32_t var_3;
    generic32_t var_4;
    generic32_t var_5;
    generic32_t *var_6;
    var_6 = &stack.offset_4;
    var_3 = *(generic32_t *) ((&stack)[1].offset_4 + 12);
    var_4 = (&stack)[1].offset_4 + 12;
    var_5 = (&stack)[1].offset_4;
    generic32_t var_7;
    while (true) {
      var_7 = var_6;
      if (var_3 != *(generic32_t *) (var_7 + 36)) {
        generic32_t var_8;
        *(generic32_t *) ((pointer_or_number32_t) var_6 + 8) = *(generic32_t *) (var_3 * 12 + *(generic32_t *) (var_5 + 8));
        *(generic32_t *) var_4 = *(generic32_t *) (var_3 * 12 + *(generic32_t *) (var_5 + 8));
        var_8 = var_6;
        if (!*(generic32_t *) (var_3 * 12 + *(generic32_t *) (var_5 + 8) + 4)) {
          generic32_t var_9;
          var_8 = (pointer_or_number32_t) var_6 - 4;
          *(generic32_t *) var_8 = 257;
          function_0x403872_Code_x86();
          var_9 = ((rawfunction_133 *) *(generic32_t *) (var_3 * 12 + *(generic32_t *) (var_5 + 8) + 8))(*(generic32_t *) (var_3 * 12 + *(generic32_t *) (var_5 + 8)));
        }
        var_7 = var_8;
        var_5 = *(generic32_t *) (var_7 + 32);
        var_4 = var_5 + 12;
        var_3 = *(generic32_t *) var_4;
        if (var_3 != (pointer_or_number32_t) -1) {
          continue;
        }
      }
      break;
    }
    var_2 = var_7;
    *(generic32_t *) NULL = *(generic32_t *) var_2;
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
void function_0x403872_Code_x86(void) {
  *(generic32_t *) (revng_undefined_local_sp() - 4) = (pointer_or_number32_t) &segment_2.offset_10236 + 20;
  *(generic32_t *) (generic32_t) 4237336 = *(generic32_t *) NULL;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x403890_Code_x86(generic32_t argument_0, generic32_t argument_1) {
  struct_348 stack;
  struct_578 *var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  var_1 = argument_0;
  var_2 = argument_1;
  stack.offset_4 = argument_0;
  var_0 = function_0x4039f0_Code_x86(var_3);
  if (!var_0) {
    stack.offset_4 = var_2;
    ((cabifunction_752 *) segment_3.offset_708)();
    var_4 = var_0;
    return var_4;
  }
  var_4 = 4294967295;
  switch ((number32_t) var_0->offset_8) {
    case 0:
    {
      stack.offset_4 = var_2;
      ((cabifunction_752 *) segment_3.offset_708)();
      var_4 = var_0;
    } break;
    case 5:
    {
      var_0->offset_8 = 0;
      var_4 = 1;
    } break;
    case 1:
    {
      break;
    } break;
    default:
    {
      generic32_t var_5;
      segment_2.offset_10408 = var_2;
      if (var_0->offset_4 == 8) {
        if ((int32_t) (segment_2.offset_10396 + segment_2.offset_10392) > (int32_t) segment_2.offset_10392) {
          generic32_t var_6;
          generic32_t var_7;
          var_7 = (pointer_or_number32_t) &segment_2.offset_10272.offset_8 + segment_2.offset_10392 * 12;
          var_6 = 0;
          do {
            var_6 = var_6 + 1;
            *(generic32_t *) var_7 = 0;
            var_7 = var_7 + 12;
          } while (segment_2.offset_10396 != var_6);
        }
        generic32_t var_8;
        var_8 = 129;
        switch ((number32_t) var_0->offset_0.member_1) {
          case 3221225613:
          case 3221225614:
          case 3221225615:
          case 3221225616:
          case 3221225617:
          case 3221225618:
          case 3221225619:
          {
            switch ((number32_t) var_0->offset_0.member_1) {
              case 3221225614:
              {
                var_8 = 131;
              } break;
              case 3221225617:
              {
                var_8 = 132;
              } break;
              case 3221225619:
              {
                var_8 = 133;
              } break;
              case 3221225615:
              {
                var_8 = 134;
              } break;
              case 3221225613:
              {
                var_8 = 130;
              } break;
              case 3221225618:
              {
                var_8 = 138;
              } break;
            }
            segment_2.offset_10404 = var_8;
          } break;
        }
        generic32_t var_9;
        generic32_t var_10;
        artificial_struct_returned_by_rawfunction_134 var_11;
        stack.offset_4 = segment_2.offset_10404;
        stack.offset_0 = 8;
        var_11 = ((rawfunction_134 *) var_0->offset_8)();
        var_10 = var_11.register_esi;
        var_5 = var_10;
        var_9 = var_11.register_edi;
        segment_2.offset_10404 = var_9;
      } else {
        generic32_t var_12;
        var_0->offset_8 = 0;
        stack.offset_4 = var_0->offset_4;
        var_12 = ((rawfunction_135 *) var_0->offset_8)();
        var_5 = var_12;
      }
      segment_2.offset_10408 = var_5;
      var_4 = 4294967295;
    } break;
  }
  return var_4;
}

_ABI(Microsoft_x86_cdecl)
struct_578 *function_0x4039f0_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_2 = &segment_2.offset_10272;
  var_1 = 0;
  generic32_t var_3;
  generic32_t var_4;
  while (true) {
    generic32_t var_5;
    var_5 = var_1;
    if (*(generic32_t *) var_2 == argument_0) {
      var_3 = var_2;
      var_4 = var_2;
    } else {
      var_2 = var_2 + 12;
      var_1 = var_5 + 1;
      if ((pointer_or_number32_t) &segment_2.offset_10272 + segment_2.offset_10400 * 12 > (pointer_or_number32_t) &segment_2.offset_10284 + var_5 * 12) {
        continue;
      }
      var_3 = (pointer_or_number32_t) &segment_2.offset_10284 + var_5 * 12;
      var_4 = (pointer_or_number32_t) &segment_2.offset_10284 + var_5 * 12;
    }
    break;
  }
  generic32_t var_6;
  var_6 = *(generic32_t *) var_3 == argument_0 ? var_4 : 0;
  return (struct_578 *) var_6;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x403a20_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  var_1 = argument_0;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = 4;
  *(generic32_t *) (revng_undefined_local_sp() - 8) = 0;
  *(generic32_t *) (revng_undefined_local_sp() - 12) = argument_0;
  var_0 = function_0x403a40_Code_x86((number8_t) var_2, var_3, var_4);
  return var_0;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x403a40_Code_x86(generic8_t argument_0, generic32_t argument_1, generic32_t argument_2) {
  generic8_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  var_3 = 1;
  if (!(argument_2 & *(generic8_t *) (*(generic8_t *) ((number64_t) &var_0 & 0xFFFFFFFF) + ((pointer_or_number32_t) &segment_2.offset_10416 + 9)))) {
    var_3 = 0;
    if (var_1) {
      var_3 = (var_1 & *(generic16_t *) ((pointer_or_number32_t) &segment_2.offset_9712 + 10 + *(generic8_t *) ((number64_t) &var_0 & 0xFFFFFFFF) * 2)) != 0;
    }
  }
  return var_3;
}

_ABI(Microsoft_x86_cdecl)
void function_0x403a80_Code_x86(void) {
  struct_358 stack;
  generic32_t var_0;
  var_0 = 4;
  if (*(generic8_t *) *(generic32_t *) &segment_2.offset_9680) {
    generic8_t var_1;
    generic32_t var_2;
    generic32_t var_3;
    var_1 = *(generic8_t *) *(generic32_t *) &segment_2.offset_9680;
    var_2 = *(generic32_t *) &segment_2.offset_9680;
    var_3 = 0;
    generic32_t var_4;
    generic32_t var_5;
    do {
      generic32_t var_6;
      generic32_t var_7;
      var_7 = var_2;
      var_4 = var_3 + (var_1 != '=');
      var_6 = 0;
      while (true) {
        var_5 = 4294967294 - var_6;
        if (*(generic8_t *) var_7) {
          generic8_t var_8;
          var_7 = var_7 + 1;
          var_8 = var_6 == (pointer_or_number32_t) -2;
          var_6 = var_6 + 1;
          var_5 = 0;
          if (!(var_8)) {
            continue;
          }
        }
        break;
      }
      var_2 = var_2 + ~var_5;
      var_1 = *(generic8_t *) var_2;
      var_3 = var_4;
    } while (var_1);
    var_0 = (var_4 << 2) + 4;
  }
  union_418 *var_9;
  generic32_t var_10;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = var_0;
  var_9 = function_0x404f00_Code_x86(var_10);
  segment_2.offset_9632 = var_9;
  if (!var_9) {
    generic32_t var_11;
    *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 9;
    function_0x4029f0_Code_x86(var_11);
  }
  generic32_t var_12;
  var_12 = var_9;
  if (*(generic8_t *) *(generic32_t *) &segment_2.offset_9680) {
    generic32_t var_13;
    union_418 *var_14;
    var_13 = *(generic32_t *) &segment_2.offset_9680;
    var_14 = var_9;
    while (true) {
      generic32_t var_15;
      generic32_t var_16;
      var_16 = var_13;
      var_15 = 0;
      generic32_t var_17;
      while (true) {
        var_17 = 4294967294 - var_15;
        if (*(generic8_t *) var_16) {
          generic8_t var_18;
          var_16 = var_16 + 1;
          var_18 = var_15 == (pointer_or_number32_t) -2;
          var_15 = var_15 + 1;
          var_17 = 0;
          if (!(var_18)) {
            continue;
          }
        }
        break;
      }
      generic32_t var_19;
      stack.offset_16 = ~var_17;
      var_19 = var_14;
      if (*(generic8_t *) var_13 != '=') {
        union_418 *var_20;
        generic32_t var_21;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = ~var_17;
        var_20 = function_0x404f00_Code_x86(var_21);
        var_14->member_0.offset_0.member_3 = var_20;
        if (!var_20) {
          generic32_t var_22;
          *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 9;
          function_0x4029f0_Code_x86(var_22);
        }
        generic32_t var_23;
        generic32_t var_24;
        generic32_t var_25;
        var_23 = 0;
        var_24 = 4294967295;
        var_25 = var_13;
        generic32_t var_26;
        generic32_t var_27;
        while (true) {
          var_27 = var_25;
          var_26 = 0;
          if (var_24) {
            generic8_t var_28;
            var_26 = 4294967294 - var_23;
            var_27 = var_13 + 1 + var_23;
            var_28 = *(generic8_t *) var_25;
            var_25 = var_25 + 1;
            var_24 = var_24 - 1;
            var_23 = var_23 + 1;
            if (var_28) {
              continue;
            }
          }
          break;
        }
        generic32_t var_29;
        generic32_t var_30;
        var_30 = var_27 - ~var_26;
        var_29 = var_20;
        if (!(var_26 > (uint32_t) -5)) {
          generic32_t var_31;
          generic32_t var_32;
          generic32_t var_33;
          var_31 = 0;
          var_32 = var_27 - ~var_26;
          var_33 = var_20;
          do {
            var_31 = var_31 + 1;
            ((union_418 *) var_33)->member_0.offset_0.member_3 = *(generic32_t *) var_32;
            var_32 = var_32 + 4;
            var_33 = &((union_418 *) var_33)->member_0.offset_4;
          } while (~var_26 >> 2 != var_31);
          var_29 = (pointer_or_number32_t) var_20 + (~var_26 & 0xFFFFFFFC);
          var_30 = var_27 + 1 + var_26 + (~var_26 & 0xFFFFFFFC);
        }
        var_19 = &var_14->member_0.offset_4;
        if ((~var_26 & 0x3)) {
          generic32_t var_34;
          generic32_t var_35;
          generic32_t var_36;
          var_34 = ~var_26 & 0x3;
          var_35 = var_30;
          var_36 = var_29;
          do {
            ((union_418 *) var_36)->member_0.offset_0.member_0 = *(generic8_t *) var_35;
            var_35 = var_35 + 1;
            var_34 = var_34 - 1;
            var_36 = &((union_418 *) var_36)->member_0.offset_0.member_1.offset_1;
          } while (var_34 != 0);
          var_19 = &var_14->member_0.offset_4;
        }
      }
      var_13 = var_13 + stack.offset_16;
      if (*(generic8_t *) var_13) {
        continue;
      }
      var_12 = var_19;
      break;
    }
  }
  generic32_t var_37;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = *(generic32_t *) &segment_2.offset_9680;
  function_0x404eb0_Code_x86(var_37);
  *(generic32_t *) &segment_2.offset_9680 = 0;
  ((union_418 *) var_12)->member_0.offset_0.member_3 = 0;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x403b70_Code_x86(void) {
  struct_360 stack;
  generic32_t var_0;
  stack.offset_8 = 260;
  stack.offset_4 = (pointer_or_number32_t) &segment_2.offset_14872 + 800;
  stack.offset_0 = 0;
  ((cabifunction_753 *) segment_3.offset_572)();
  segment_2.offset_9648 = (pointer_or_number32_t) &segment_2.offset_14872 + 800;
  var_0 = (pointer_or_number32_t) &segment_2.offset_14872 + 800;
  if (*segment_2.offset_20324) {
    var_0 = segment_2.offset_20324;
  }
  union_418 *var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  *(generic32_t **) ((pointer_or_number32_t) &stack - 4) = &stack.offset_12;
  *(generic32_t **) ((pointer_or_number32_t) &stack - 8) = &stack.offset_8;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = 0;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 16) = 0;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 20) = var_0;
  function_0x403c10_Code_x86((struct_599 *) var_2, (struct_578 **) var_3, (struct_578 *) var_4, (generic32_t *) var_5, (struct_578 **) var_6);
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = (stack.offset_8 << 2) + stack.offset_12;
  var_1 = function_0x404f00_Code_x86(var_7);
  if (!var_1) {
    generic32_t var_8;
    *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 8;
    function_0x4029f0_Code_x86(var_8);
  }
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  *(generic32_t **) ((pointer_or_number32_t) &stack - 4) = &stack.offset_12;
  *(generic32_t **) ((pointer_or_number32_t) &stack - 8) = &stack.offset_8;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = (stack.offset_8 << 2) + (pointer_or_number32_t) var_1;
  *(union_418 **) ((pointer_or_number32_t) &stack - 16) = var_1;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 20) = var_0;
  function_0x403c10_Code_x86((struct_599 *) var_9, (struct_578 **) var_10, (struct_578 *) var_11, (generic32_t *) var_12, (struct_578 **) var_13);
  segment_2.offset_9624 = var_1;
  segment_2.offset_9620 = stack.offset_8 - 1;
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl)
void function_0x403c10_Code_x86(struct_599 *argument_0, struct_578 **argument_1, struct_578 *argument_2, generic32_t *argument_3, struct_578 **argument_4) {
  uint64_t loop_state_var;
  struct_599 *var_0;
  generic32_t var_1;
  struct_578 *var_2;
  generic32_t *var_3;
  struct_578 **var_4;
  generic32_t var_5;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  var_3 = argument_3;
  var_4 = argument_4;
  *argument_4 = 0;
  *argument_3 = 1;
  var_5 = argument_3;
  if (argument_1) {
    var_5 = var_1;
    var_1 = var_5 + 4;
    *(struct_578 **) var_5 = argument_2;
  }
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  if (argument_0->offset_0 == '\"') {
    generic32_t var_9;
    generic32_t var_10;
    generic32_t var_11;
    var_9 = &argument_0->offset_1;
    var_10 = var_5;
    var_11 = argument_2;
    switch ((number8_t) argument_0->offset_1) {
      case 0:
      case 34:
      {
        break;
      } break;
      default:
      {
        generic32_t var_12;
        generic32_t var_13;
        generic32_t var_14;
        struct_578 *var_15;
        var_13 = argument_0->offset_1;
        var_12 = var_13;
        var_14 = &argument_0->offset_1;
        var_15 = argument_2;
        generic32_t var_16;
        generic32_t var_17;
        generic32_t var_18;
        while (true) {
          generic32_t var_19;
          generic32_t var_20;
          generic32_t var_21;
          var_20 = var_13;
          var_18 = var_14;
          var_21 = var_15;
          var_19 = var_18;
          if ((*(generic8_t *) ((pointer_or_number32_t) &segment_2.offset_10416 + 9 + var_20 * 1) & 0x4)) {
            *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
            var_18 = var_14;
            var_20 = var_13;
            var_21 = 0;
            if (var_15) {
              var_20 = *(generic8_t *) var_19;
              var_18 = var_14 + 1;
              var_15->offset_0.member_0.offset_0 = *(generic8_t *) var_19;
              var_21 = &var_15->offset_0.member_0.offset_1;
            }
          }
          var_16 = var_20;
          *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
          var_17 = 0;
          if (var_21) {
            var_16 = *(generic8_t *) var_18;
            ((struct_578 *) var_21)->offset_0.member_0.offset_0 = *(generic8_t *) var_18;
            var_17 = &((struct_578 *) var_21)->offset_0.member_0.offset_1;
          }
          if (*(generic8_t *) (var_18 + 1) != '\"') {
            var_13 = *(generic8_t *) (var_18 + 1);
            var_12 = (var_12 & 0xFFFFFF00) | var_13;
            var_14 = var_18 + 1;
            if (*(generic8_t *) (var_18 + 1)) {
              continue;
            }
          }
          break;
        }
        var_9 = var_18 + 1;
        var_10 = var_16;
        var_11 = var_17;
      } break;
    }
    generic32_t var_22;
    *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
    var_22 = 0;
    if (var_11) {
      ((struct_578 *) var_11)->offset_0.member_0.offset_0 = '\000';
      var_22 = &((struct_578 *) var_11)->offset_0.member_0.offset_1;
    }
    var_6 = var_22;
    var_8 = var_9 + (*(generic8_t *) var_9 == '\"');
    var_7 = var_10;
  } else {
    struct_578 *var_23;
    generic32_t var_24;
    struct_599 *var_25;
    var_23 = argument_2;
    var_24 = var_5;
    var_25 = argument_0;
    while (true) {
      generic32_t var_26;
      generic32_t var_27;
      *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
      if (!var_23) {
        var_26 = (var_24 & 0xFFFFFF00) | var_25->offset_0;
        var_27 = 0;
      } else {
        var_23->offset_0.member_0.offset_0 = var_25->offset_0;
        var_26 = (var_24 & 0xFFFFFF00) | var_25->offset_0;
        var_27 = &var_23->offset_0.member_0.offset_1;
      }
      generic32_t var_28;
      generic32_t var_29;
      var_28 = var_27;
      var_29 = &var_25->offset_1;
      if ((*(generic8_t *) ((pointer_or_number32_t) &segment_2.offset_10416 + 9 + (var_26 & 0xFF) * 1) & 0x4)) {
        generic32_t var_30;
        *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
        var_30 = 0;
        if (var_27) {
          *(generic8_t *) var_27 = var_25->offset_1;
          var_30 = var_27 + 1;
        }
        var_28 = var_30;
        var_29 = &var_25[1];
      }
      bool var_31 = false;
      switch ((number8_t) var_26) {
        case 0:
        case 9:
        case 32:
        {
          switch ((number8_t) var_26) {
            case 9:
            case 32:
            {
              var_6 = 0;
              var_7 = var_26;
              var_8 = var_29;
              if (var_28) {
                *(generic8_t *) (var_28 - 1) = '\000';
                var_6 = var_28;
                var_7 = var_26;
                var_8 = var_29;
              }
            } break;
            case 0:
            {
              var_8 = var_29 - 1;
              var_6 = var_28;
              var_7 = var_26;
            } break;
          }
          var_31 = true;
          break;
        } break;
        default:
        {
        } break;
      }
      if (var_31){
        break;}
    }
  }
  if (*(generic8_t *) var_8) {
    generic32_t var_32;
    generic32_t var_33;
    generic32_t var_34;
    generic32_t var_35;
    generic32_t var_36;
    var_32 = var_8;
    var_33 = var_7;
    var_34 = 0;
    var_35 = var_6;
    var_36 = 0;
    while (true) {
      generic32_t var_37;
      generic32_t var_38;
      var_38 = var_32;
      var_37 = var_33;
      while (true) {
        generic8_t var_39;
        bool var_40 = false;
        var_39 = *(generic8_t *) var_38;
        switch ((number8_t) var_39) {
          case 9:
          case 32:
          {
            var_38 = var_38 + 1;
            var_37 = (var_37 & 0xFFFFFF00) | var_39;
          } break;
          case 0:
          {
            var_40 = true;
            break;
          } break;
          default:
          {
            loop_state_var = 1;
            var_40 = true;
            break;
          } break;
        }
        if (var_40){
          break;}
      }
      if (loop_state_var == 1) {
        generic32_t var_41;
        var_41 = var_1;
        if (var_41) {
          var_1 = var_41 + 4;
          *(generic32_t *) var_41 = var_35;
        }
        generic32_t var_42;
        generic32_t var_43;
        generic32_t var_44;
        generic32_t var_45;
        *var_3 = *var_3 + 1;
        var_42 = var_36;
        var_43 = var_35;
        var_44 = var_34;
        var_45 = var_38;
        generic32_t var_46;
        generic32_t var_47;
        while (true) {
          generic8_t var_48;
          generic32_t var_49;
          generic32_t var_50;
          var_50 = var_45;
          var_48 = *(generic8_t *) var_50;
          var_49 = 0;
          if (var_48 == '\\') {
            generic32_t var_51;
            var_51 = 0;
            generic32_t var_52;
            do {
              var_52 = var_45 + 1 + var_51;
              var_51 = var_51 + 1;
            } while (*(generic8_t *) var_52 == '\\');
            var_48 = *(generic8_t *) var_52;
            var_49 = var_51;
            var_50 = var_52;
          }
          generic32_t var_53;
          generic32_t var_54;
          generic32_t var_55;
          generic32_t var_56;
          var_55 = var_49;
          var_47 = var_50;
          var_53 = var_44;
          var_54 = 1;
          var_56 = var_42;
          if (var_48 == '\"') {
            generic32_t var_57;
            generic32_t var_58;
            generic32_t var_59;
            generic32_t var_60;
            var_57 = var_42;
            var_58 = 1;
            var_59 = var_44;
            var_60 = var_50;
            if (!(var_49 & 0x1)) {
              generic32_t var_61;
              generic32_t var_62;
              var_61 = 0;
              var_62 = var_50;
              if (var_44) {
                generic32_t var_63;
                var_61 = *(generic8_t *) (var_50 + 1) == '\"';
                var_63 = *(generic8_t *) (var_50 + 1) == '\"' ? var_50 + 1 : var_50;
                var_62 = var_63;
              }
              var_58 = var_61;
              var_60 = var_62;
              var_59 = !var_44;
              var_57 = var_59;
            }
            var_56 = var_57;
            var_54 = var_58;
            var_53 = var_59;
            var_47 = var_60;
            var_55 = var_49 >> 1;
          }
          var_46 = var_43;
          if (var_55) {
            generic32_t var_64;
            generic32_t var_65;
            var_64 = 0;
            var_65 = var_43;
            generic8_t var_66;
            generic32_t var_67;
            do {
              var_67 = 0;
              if (var_65) {
                *(generic8_t *) var_65 = '\\';
                var_67 = var_65 + 1;
              }
              *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
              var_66 = var_55 == var_64 + 1;
              var_64 = var_64 + 1;
            } while (!(var_66));
            var_46 = var_67;
          }
          if (!*(generic8_t *) var_47) {
            break;
          }
          if (!var_53) {
            bool var_68 = false;
            switch ((number8_t) *(generic8_t *) var_47) {
              case 9:
              case 32:
              {
                var_68 = true;
                break;
              } break;
            }
            if (var_68){
              break;}
          }
          generic32_t var_69;
          generic32_t var_70;
          var_69 = var_46;
          var_70 = var_47;
          if (var_54) {
            generic32_t var_71;
            generic32_t var_72;
            if (!var_46) {
              var_71 = 0;
              var_72 = var_47;
              if ((*(generic8_t *) (*(generic8_t *) var_47 + ((pointer_or_number32_t) &segment_2.offset_10416 + 9)) & 0x4)) {
                var_72 = var_47 + 1;
                *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
                var_71 = 0;
              }
            } else {
              generic32_t var_73;
              generic32_t var_74;
              generic32_t var_75;
              var_73 = var_47;
              var_74 = var_47;
              var_75 = var_46;
              if ((*(generic8_t *) (*(generic8_t *) var_47 + ((pointer_or_number32_t) &segment_2.offset_10416 + 9)) & 0x4)) {
                *(generic8_t *) var_46 = *(generic8_t *) var_47;
                var_74 = var_47 + 1;
                var_75 = var_46 + 1;
                *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
                var_73 = var_74;
              }
              var_72 = var_74;
              var_71 = var_75 + 1;
              *(generic8_t *) var_75 = *(generic8_t *) var_73;
            }
            var_69 = var_71;
            var_70 = var_72;
            *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
          }
          var_45 = var_70 + 1;
        }
        generic32_t var_76;
        var_76 = 0;
        if (var_46) {
          *(generic8_t *) var_46 = '\000';
          var_76 = var_46 + 1;
        }
        *argument_4 = &(*argument_4)->offset_0.member_0.offset_1;
        var_33 = *(generic8_t *) var_47;
        if (*(generic8_t *) var_47) {
          continue;
        }
      }
      break;
    }
  }
  if (var_1) {
    *(generic32_t *) var_1 = 0;
  }
  *var_3 = *var_3 + 1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x403df0_Code_x86(void) {
  struct_361 stack;
  union_418 *var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  union_418 *var_7;
  generic32_t var_8;
  generic32_t var_9;
  generic32_t var_10;
  generic32_t var_11;
  generic32_t var_12;
  generic32_t var_13;
  generic32_t var_14;
  generic32_t var_15;
  generic32_t var_16;
  generic32_t var_17;
  generic32_t var_18;
  generic32_t var_19;
  generic32_t var_20;
  generic32_t var_21;
  generic32_t var_22;
  generic32_t var_23;
  generic32_t var_24;
  generic32_t var_25;
  generic32_t var_26;
  generic32_t var_27;
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  generic32_t var_31;
  generic32_t var_32;
  generic32_t var_33;
  generic32_t var_34;
  generic32_t var_35;
  artificial_struct_returned_by_rawfunction_142 var_36;
  artificial_struct_returned_by_rawfunction_143 var_37;
  var_35 = segment_3.offset_728;
  var_15 = 0;
  var_29 = 0;
  switch ((number32_t) segment_2.offset_10416) {
    case 0:
    {
      generic8_t var_38;
      generic32_t var_39;
      generic32_t var_40;
      generic32_t var_41;
      generic32_t var_42;
      generic32_t var_43;
      generic32_t var_44;
      generic32_t var_45;
      artificial_struct_returned_by_rawfunction_139 var_46;
      var_46 = ((rawfunction_139 *) segment_3.offset_728)();
      var_41 = var_46.register_eax;
      var_44 = var_41;
      var_40 = var_46.register_ebx;
      var_43 = var_40;
      var_39 = var_46.register_esi;
      var_45 = var_39;
      var_38 = var_44 == var_43;
      var_42 = 1;
      if (var_38) {
        generic32_t var_47;
        generic32_t var_48;
        generic32_t var_49;
        artificial_struct_returned_by_rawfunction_140 var_50;
        var_50 = ((rawfunction_140 *) segment_3.offset_704)();
        var_49 = var_50.register_eax;
        var_43 = var_49;
        var_48 = var_50.register_esi;
        var_45 = var_48;
        var_47 = var_50.register_edi;
        var_44 = var_47;
        var_15 = 0;
        var_42 = 2;
        if (!var_43) {
          return var_15;
        }
      }
      segment_2.offset_10416 = var_42;
      if (var_38) {
        var_27 = var_44;
        var_28 = var_43;
        var_29 = var_44;
        if (!var_43) {
          var_27 = var_29;
          var_1 = ((cabifunction_756 *) segment_3.offset_704)();
          var_28 = var_1;
          var_15 = 0;
          if (!var_28) {
            return var_15;
          }
        }
        var_24 = var_28;
        if (*(generic8_t *) var_24) {
          var_26 = var_28;
          while (true) {
            var_25 = var_26 + 1;
            if (!*(generic8_t *) var_25) {
              var_25 = var_26 + 2;
              if (!*(generic8_t *) var_25) {
                break;
              }
            }
          }
          var_24 = var_26 + 2;
        }
        *(generic32_t *) &stack.offset_64 = var_24 - var_28 + 1;
        var_0 = function_0x404f00_Code_x86(var_13);
        stack.offset_84 = var_0;
        if (!var_0) {
          *(generic32_t *) &stack.offset_64 = var_28;
          ((rawfunction_147 *) segment_3.offset_656)(undef(generic32_t), var_27);
          var_15 = 0;
        } else {
          var_19 = var_0;
          var_20 = var_28;
          if (!(var_24 - var_28 < 3 || var_24 - var_28 > (uint32_t) -2)) {
            var_21 = 0;
            var_22 = var_28;
            var_23 = var_0;
            do {
              var_21 = var_21 + 1;
              ((union_418 *) var_23)->member_0.offset_0.member_3 = *(generic32_t *) var_22;
              var_22 = var_22 + 4;
              var_23 = &((union_418 *) var_23)->member_0.offset_4;
            } while ((var_24 - var_28 + 1) >> 2 != var_21);
            var_19 = (pointer_or_number32_t) var_0 + ((var_24 - var_28 + 1) & 0xFFFFFFFC);
            var_20 = var_28 + ((var_24 - var_28 + 1) & 0xFFFFFFFC);
          }
          *(generic32_t *) &stack.offset_64 = var_28;
          if (((var_24 - var_28 + 1) & 0x3)) {
            var_16 = (var_24 - var_28 + 1) & 0x3;
            var_17 = var_20;
            var_18 = var_19;
            do {
              ((union_418 *) var_18)->member_0.offset_0.member_0 = *(generic8_t *) var_17;
              var_17 = var_17 + 1;
              var_16 = var_16 - 1;
              var_18 = &((union_418 *) var_18)->member_0.offset_0.member_1.offset_1;
            } while (var_16 != 0);
          }
          ((cabifunction_757 *) segment_3.offset_656)();
          var_15 = stack.offset_80;
        }
        return var_15;
      }
      var_34 = var_44;
      var_35 = var_45;
      if (!var_44) {
        var_11 = ((cabifunction_754 *) var_35)();
        var_34 = var_11;
        var_15 = 0;
        if (!var_34) {
          return var_15;
        }
      }
      var_31 = var_34;
      if (*(generic16_t *) var_31) {
        var_33 = var_34;
        while (true) {
          var_32 = var_33 + 2;
          if (!*(generic16_t *) var_32) {
            var_32 = var_33 + 4;
            if (!*(generic16_t *) var_32) {
              break;
            }
          }
        }
        var_31 = var_33 + 4;
      }
      *(generic32_t *) &stack.offset_64 = 0;
      stack.offset_60 = 0;
      stack.offset_56 = 0;
      stack.offset_52 = 0;
      stack.offset_48 = ((int32_t) (var_31 - var_34) >> 1) + 1;
      stack.offset_44 = var_34;
      stack.offset_40 = 0;
      stack.offset_36 = 0;
      var_36 = ((rawfunction_142 *) segment_3.offset_724)();
      var_10 = var_36.register_eax;
      var_9 = var_36.register_esi;
      var_8 = var_36.register_edi;
      if (!var_10) {
        stack.offset_32 = var_8;
        ((cabifunction_755 *) segment_3.offset_732)();
        revng_abort("A longjmp was taken");
      } else {
        stack.offset_32 = var_10;
        var_7 = function_0x404f00_Code_x86(var_12);
        if (!var_7) {
          stack.offset_32 = var_8;
          ((cabifunction_755 *) segment_3.offset_732)();
          revng_abort("A longjmp was taken");
        } else {
          stack.offset_32 = 0;
          stack.offset_28 = 0;
          stack.offset_24 = var_10;
          stack.offset_20 = var_7;
          stack.offset_16 = var_9;
          stack.offset_12 = var_8;
          stack.offset_8 = 0;
          stack.offset_4 = 0;
          var_37 = ((rawfunction_143 *) segment_3.offset_724)();
          var_6 = var_37.register_eax;
          var_5 = var_37.register_ebx;
          var_30 = var_5;
          var_4 = var_37.register_ecx;
          var_3 = var_37.register_edi;
          if (!var_6) {
            stack.offset_0 = var_5;
            function_0x404eb0_Code_x86(var_14);
            var_30 = 0;
            stack.offset_0 = var_3;
            var_2 = ((rawfunction_144 *) segment_3.offset_732)(var_30);
            revng_abort("A longjmp was taken");
          } else {
            stack.offset_0 = var_3;
            var_2 = ((rawfunction_144 *) segment_3.offset_732)(var_30);
            revng_abort("A longjmp was taken");
          }
        }
      }
    } break;
    case 1:
    {
      var_11 = ((cabifunction_754 *) var_35)();
      var_34 = var_11;
      var_15 = 0;
      if (!var_34) {
        return var_15;
      }
      var_31 = var_34;
      if (*(generic16_t *) var_31) {
        var_33 = var_34;
        while (true) {
          var_32 = var_33 + 2;
          if (!*(generic16_t *) var_32) {
            var_32 = var_33 + 4;
            if (!*(generic16_t *) var_32) {
              break;
            }
          }
        }
        var_31 = var_33 + 4;
      }
      *(generic32_t *) &stack.offset_64 = 0;
      stack.offset_60 = 0;
      stack.offset_56 = 0;
      stack.offset_52 = 0;
      stack.offset_48 = ((int32_t) (var_31 - var_34) >> 1) + 1;
      stack.offset_44 = var_34;
      stack.offset_40 = 0;
      stack.offset_36 = 0;
      var_36 = ((rawfunction_142 *) segment_3.offset_724)();
      var_10 = var_36.register_eax;
      var_9 = var_36.register_esi;
      var_8 = var_36.register_edi;
      if (!var_10) {
        stack.offset_32 = var_8;
        ((cabifunction_755 *) segment_3.offset_732)();
        revng_abort("A longjmp was taken");
      } else {
        stack.offset_32 = var_10;
        var_7 = function_0x404f00_Code_x86(var_12);
        if (!var_7) {
          stack.offset_32 = var_8;
          ((cabifunction_755 *) segment_3.offset_732)();
          revng_abort("A longjmp was taken");
        } else {
          stack.offset_32 = 0;
          stack.offset_28 = 0;
          stack.offset_24 = var_10;
          stack.offset_20 = var_7;
          stack.offset_16 = var_9;
          stack.offset_12 = var_8;
          stack.offset_8 = 0;
          stack.offset_4 = 0;
          var_37 = ((rawfunction_143 *) segment_3.offset_724)();
          var_6 = var_37.register_eax;
          var_5 = var_37.register_ebx;
          var_30 = var_5;
          var_4 = var_37.register_ecx;
          var_3 = var_37.register_edi;
          if (!var_6) {
            stack.offset_0 = var_5;
            function_0x404eb0_Code_x86(var_14);
            var_30 = 0;
            stack.offset_0 = var_3;
            var_2 = ((rawfunction_144 *) segment_3.offset_732)(var_30);
            revng_abort("A longjmp was taken");
          } else {
            stack.offset_0 = var_3;
            var_2 = ((rawfunction_144 *) segment_3.offset_732)(var_30);
            revng_abort("A longjmp was taken");
          }
        }
      }
    } break;
    default:
    {
      if ((number32_t) segment_2.offset_10416 != 2) {
        return var_15;
      }
      var_27 = var_29;
      var_1 = ((cabifunction_756 *) segment_3.offset_704)();
      var_28 = var_1;
      var_15 = 0;
      if (!var_28) {
        return var_15;
      }
      var_24 = var_28;
      if (*(generic8_t *) var_24) {
        var_26 = var_28;
        while (true) {
          var_25 = var_26 + 1;
          if (!*(generic8_t *) var_25) {
            var_25 = var_26 + 2;
            if (!*(generic8_t *) var_25) {
              break;
            }
          }
        }
        var_24 = var_26 + 2;
      }
      *(generic32_t *) &stack.offset_64 = var_24 - var_28 + 1;
      var_0 = function_0x404f00_Code_x86(var_13);
      stack.offset_84 = var_0;
      if (!var_0) {
        *(generic32_t *) &stack.offset_64 = var_28;
        ((rawfunction_147 *) segment_3.offset_656)(undef(generic32_t), var_27);
        var_15 = 0;
      } else {
        var_19 = var_0;
        var_20 = var_28;
        if (!(var_24 - var_28 < 3 || var_24 - var_28 > (uint32_t) -2)) {
          var_21 = 0;
          var_22 = var_28;
          var_23 = var_0;
          do {
            var_21 = var_21 + 1;
            ((union_418 *) var_23)->member_0.offset_0.member_3 = *(generic32_t *) var_22;
            var_22 = var_22 + 4;
            var_23 = &((union_418 *) var_23)->member_0.offset_4;
          } while ((var_24 - var_28 + 1) >> 2 != var_21);
          var_19 = (pointer_or_number32_t) var_0 + ((var_24 - var_28 + 1) & 0xFFFFFFFC);
          var_20 = var_28 + ((var_24 - var_28 + 1) & 0xFFFFFFFC);
        }
        *(generic32_t *) &stack.offset_64 = var_28;
        if (((var_24 - var_28 + 1) & 0x3)) {
          var_16 = (var_24 - var_28 + 1) & 0x3;
          var_17 = var_20;
          var_18 = var_19;
          do {
            ((union_418 *) var_18)->member_0.offset_0.member_0 = *(generic8_t *) var_17;
            var_17 = var_17 + 1;
            var_16 = var_16 - 1;
            var_18 = &((union_418 *) var_18)->member_0.offset_0.member_1.offset_1;
          } while (var_16 != 0);
        }
        ((cabifunction_757 *) segment_3.offset_656)();
        var_15 = stack.offset_80;
      }
      return var_15;
    } break;
  }
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x403f80_Code_x86(generic32_t argument_0) {
  struct_362 stack;
  uint64_t loop_state_var;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_1 = argument_0;
  stack.offset_4 = argument_0;
  var_0 = function_0x404160_Code_x86(var_2);
  if (var_0 != (pointer_or_number32_t) segment_2.offset_10684) {
    if (!var_0) {
      function_0x404210_Code_x86();
    } else {
      generic32_t var_3;
      generic32_t var_4;
      *(generic32_t *) &stack.offset_24 = 0;
      var_4 = &segment_2.offset_10720;
      var_3 = 0;
      generic64_t var_5;
      generic32_t var_6;
      generic32_t var_7;
      generic64_t var_8;
      generic32_t var_9;
      generic32_t var_10;
      while (true) {
        generic32_t var_11;
        var_11 = var_3;
        if (*(generic32_t *) var_4 == var_0) {
          var_5 = 0;
          var_6 = 0;
          var_7 = (generic32_t) 4237496;
          break;
        }
        var_3 = var_11 + 1;
        var_4 = var_4 + 48;
        *(generic32_t *) &stack.offset_24 = var_3;
        if (!(var_11 * 48 > 191)) {
          continue;
        }
        generic32_t var_12;
        stack.offset_4 = &stack.offset_28;
        stack.offset_0 = var_0;
        var_12 = ((cabifunction_758 *) segment_3.offset_676)();
        if (var_12 == 1) {
          var_8 = 0;
          var_9 = 0;
          var_10 = (generic32_t) 4237496;
          loop_state_var = 0;
          break;
        }
        if (!segment_2.offset_10708) {
          revng_abort("A longjmp was taken");
        } else {
          function_0x404210_Code_x86();
          revng_abort("A longjmp was taken");
        }
      }
      if (!(loop_state_var)) {
        do {
          *(generic32_t *) var_10 = 0;
          var_9 = var_9 + 1;
          var_10 = (var_8 << 2) + 4237500;
          var_8 = var_8 + 1;
        } while (var_9 != 64);
        generic32_t var_13;
        *(generic8_t *) var_10 = '\000';
        if (stack.offset_20 > 1) {
          if (stack.offset_24.member_0.offset_2) {
            generic32_t var_14;
            generic8_t var_15;
            generic32_t var_16;
            var_14 = 0;
            var_15 = stack.offset_24.member_0.offset_2;
            var_16 = 0;
            while (true) {
              if (*(generic8_t *) ((pointer_or_number32_t) &stack.offset_24.member_1.offset_3 + var_14 * 2)) {
                generic32_t var_17;
                var_17 = (var_16 & 0xFFFFFF00) | *(generic8_t *) ((pointer_or_number32_t) &stack.offset_24.member_1.offset_3 + var_14 * 2);
                if (!(*(generic8_t *) ((pointer_or_number32_t) &stack.offset_24.member_1.offset_3 + var_14 * 2) < var_15)) {
                  generic64_t var_18;
                  generic32_t var_19;
                  var_18 = 0;
                  var_19 = 0;
                  generic8_t var_20;
                  do {
                    *(generic8_t *) (var_15 + 4237497 + var_18) = *(generic8_t *) (var_15 + 4237497 + var_18) | 0x4;
                    var_20 = var_15 + 1 + var_19 > *(generic8_t *) ((pointer_or_number32_t) &stack.offset_24.member_1.offset_3 + var_14 * 2);
                    var_19 = var_19 + 1;
                    var_18 = var_18 + 1;
                  } while (!(var_20));
                  var_17 = *(generic8_t *) ((pointer_or_number32_t) &stack.offset_24.member_1.offset_3 + var_14 * 2);
                }
                var_15 = *(generic8_t *) ((pointer_or_number32_t) &stack.offset_28 + var_14 * 2);
                var_14 = var_14 + 1;
                if (var_15) {
                  continue;
                }
              }
              break;
            }
          }
          generic64_t var_21;
          generic32_t var_22;
          var_21 = 0;
          var_22 = 0;
          generic8_t var_23;
          do {
            *(generic8_t *) (var_21 + 4237498) = *(generic8_t *) (var_21 + 4237498) | 0x8;
            var_23 = var_22 < (uint32_t) -2 && var_22 > 252;
            var_22 = var_22 + 1;
            var_21 = var_21 + 1;
          } while (!(var_23));
          generic32_t var_24;
          generic32_t var_25;
          *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = var_0;
          segment_2.offset_10684 = var_0;
          var_24 = function_0x4041b0_Code_x86(var_25);
          var_13 = var_24;
          segment_2.offset_10688 = var_13;
          *(generic32_t *) (generic32_t) 4237768 = 0;
          *(generic32_t *) (generic32_t) 4237772 = 0;
          *(generic32_t *) (generic32_t) 4237776 = 0;
          revng_abort("A longjmp was taken");
        } else {
          segment_2.offset_10684 = 0;
          var_13 = 0;
          segment_2.offset_10688 = var_13;
          *(generic32_t *) (generic32_t) 4237768 = 0;
          *(generic32_t *) (generic32_t) 4237772 = 0;
          *(generic32_t *) (generic32_t) 4237776 = 0;
          revng_abort("A longjmp was taken");
        }
      }
      do {
        *(generic32_t *) var_7 = 0;
        var_6 = var_6 + 1;
        var_7 = (var_5 << 2) + 4237500;
        var_5 = var_5 + 1;
      } while (var_6 != 64);
      generic64_t var_26;
      generic32_t var_27;
      *(generic8_t *) var_7 = '\000';
      var_26 = 0;
      var_27 = 0;
      generic8_t var_28;
      do {
        if (*(generic8_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 16 + *(generic32_t *) &stack.offset_24 * 48 + (number32_t) var_26 * 8)) {
          if (*(generic8_t *) (((number32_t) var_26 << 3) + (((pointer_or_number32_t) &segment_2.offset_10720 + 16 + *(generic32_t *) &stack.offset_24 * 48) | 0x1))) {
            generic32_t var_29;
            generic32_t var_30;
            generic8_t var_31;
            generic32_t var_32;
            generic8_t var_33;
            var_29 = 0;
            var_30 = *(generic32_t *) &stack.offset_24 * 6 + (number32_t) var_26;
            var_31 = *(generic8_t *) (((number32_t) var_26 << 3) + (((pointer_or_number32_t) &segment_2.offset_10720 + 16 + *(generic32_t *) &stack.offset_24 * 48) | 0x1));
            var_32 = ((number32_t) var_26 << 3) + (((pointer_or_number32_t) &segment_2.offset_10720 + 16 + *(generic32_t *) &stack.offset_24 * 48) | 0x1);
            var_33 = *(generic8_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 16 + *(generic32_t *) &stack.offset_24 * 48 + (number32_t) var_26 * 8);
            while (true) {
              generic32_t var_34;
              generic8_t var_35;
              var_34 = var_29;
              var_35 = var_31;
              if (!(var_35 < var_33)) {
                generic64_t var_36;
                generic32_t var_37;
                var_36 = 0;
                var_37 = 0;
                generic8_t var_38;
                do {
                  *(generic8_t *) (var_33 + 4237497 + var_36) = *(generic8_t *) (var_33 + 4237497 + var_36) | *(generic8_t *) (var_26 + 4237784);
                  var_38 = var_33 + 1 + var_37 > *(generic8_t *) var_32;
                  var_37 = var_37 + 1;
                  var_36 = var_36 + 1;
                } while (!(var_38));
                var_35 = *(generic8_t *) (var_26 + 4237784);
              }
              if (*(generic8_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 18 + *(generic32_t *) &stack.offset_24 * 48 + var_27 * 8 + var_34 * 2)) {
                var_30 = (var_30 & 0xFFFFFF00) | var_35;
                var_31 = *(generic8_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 19 + *(generic32_t *) &stack.offset_24 * 48 + var_27 * 8 + var_34 * 2);
                var_29 = var_34 + 1;
                var_32 = (pointer_or_number32_t) &segment_2.offset_10720 + 19 + *(generic32_t *) &stack.offset_24 * 48 + var_27 * 8 + var_34 * 2;
                var_33 = *(generic8_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 18 + *(generic32_t *) &stack.offset_24 * 48 + var_27 * 8 + var_34 * 2);
                if (var_31) {
                  continue;
                }
              }
              break;
            }
          }
        }
        var_28 = var_27 < (uint32_t) -1 && var_27 > 2;
        var_27 = var_27 + 1;
        var_26 = var_26 + 1;
      } while (!(var_28));
      generic32_t var_39;
      generic32_t var_40;
      stack.offset_4 = var_0;
      segment_2.offset_10684 = var_0;
      var_39 = function_0x4041b0_Code_x86(var_40);
      segment_2.offset_10688 = var_39;
      *(generic32_t *) (generic32_t) 4237768 = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 4 + *(generic32_t *) &stack.offset_24 * 48);
      *(generic32_t *) (generic32_t) 4237772 = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 8 + *(generic32_t *) &stack.offset_24 * 48);
      *(generic32_t *) (generic32_t) 4237776 = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10720 + 12 + *(generic32_t *) &stack.offset_24 * 48);
    }
  }
  return 0;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x404160_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  segment_2.offset_10708 = 0;
  var_1 = argument_0;
  switch ((number32_t) argument_0) {
    case 4294967294:
    {
      segment_2.offset_10708 = 1;
      var_1 = 4294967294;
    } break;
    case 4294967293:
    {
      segment_2.offset_10708 = 1;
      var_1 = 4294967293;
    } break;
    case 4294967292:
    {
      segment_2.offset_10708 = 1;
      var_1 = segment_2.offset_11840;
    } break;
  }
  return var_1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4041b0_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  var_1 = argument_0;
  var_0 = argument_0 > 950 || argument_0 < 932 ? 0 : argument_0 - 932;
  return var_0;
}

_ABI(Microsoft_x86_cdecl)
struct_822 function_0x4041e8_Code_x86(void) {
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4041fc_Code_x86(void) {
}

_ABI(Microsoft_x86_cdecl)
void function_0x404210_Code_x86(void) {
  generic64_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_0 = 0;
  var_1 = 0;
  var_2 = (generic32_t) 4237496;
  do {
    *(generic32_t *) var_2 = 0;
    var_1 = var_1 + 1;
    var_2 = (var_0 << 2) + 4237500;
    var_0 = var_0 + 1;
  } while (var_1 != 64);
  *(generic8_t *) var_2 = '\000';
  segment_2.offset_10696 = 0;
  segment_2.offset_10684 = 0;
  segment_2.offset_10688 = 0;
  *(generic32_t *) (generic32_t) 4237772 = 0;
  *(generic32_t *) (generic32_t) 4237776 = 0;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x404240_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = 4294967293;
  var_0 = function_0x403f80_Code_x86(var_1);
  return var_0;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404250_Code_x86(void) {
  struct_363 stack;
  union_418 *var_0;
  generic32_t var_1;
  stack.offset_0 = 256;
  var_0 = function_0x404f00_Code_x86(var_1);
  if (!var_0) {
    generic32_t var_2;
    stack.offset_0 = 27;
    function_0x4029f0_Code_x86(var_2);
  }
  segment_2.offset_20064 = var_0;
  segment_2.offset_20320 = 32;
  if ((uint32_t) var_0 < (uint32_t) -256) {
    generic32_t var_3;
    generic32_t var_4;
    var_3 = 0;
    var_4 = var_0;
    generic8_t var_5;
    do {
      var_0->member_1.offset_0[var_3].offset_0.offset_4 = '\000';
      ((union_418 *) var_4)->member_0.offset_0.member_3 = 4294967295;
      *(generic8_t *) ((pointer_or_number32_t) &var_0->member_1.offset_0[var_3] + 5) = '\n';
      var_5 = (uint32_t) &segment_2.offset_20064[12].member_0.offset_4 > (uint32_t) &var_0->member_1.offset_0[var_3 + 1];
      var_3 = var_3 + 1;
      var_4 = &((union_418 *) var_4)->member_0.offset_8;
    } while (var_5);
  }
  generic32_t var_6;
  generic32_t var_7;
  stack.offset_0 = (pointer_or_number32_t) &stack + 20;
  ((cabifunction_762 *) segment_3.offset_752)();
  var_6 = &stack;
  var_7 = 0;
  if (stack.offset_66) {
    var_6 = &stack;
    var_7 = 0;
    if (stack.offset_68) {
      generic32_t var_8;
      generic32_t var_9;
      generic32_t var_10;
      var_10 = llvm_smin_i32(stack.offset_68->offset_0, 2048);
      var_8 = stack.offset_68;
      var_9 = 0;
      if ((int32_t) segment_2.offset_20320 < (int32_t) var_10) {
        union_418 *var_11;
        generic32_t var_12;
        generic32_t var_13;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 256;
        var_11 = function_0x404f00_Code_x86(var_12);
        var_13 = 0;
        if (!var_11) {
          var_9 = var_13;
          var_10 = segment_2.offset_20320;
          var_8 = 0;
        } else {
          union_418 *var_14;
          generic32_t var_15;
          var_15 = &segment_2.offset_20068;
          var_14 = var_11;
          while (true) {
            generic32_t var_16;
            generic32_t var_17;
            var_16 = var_14;
            *(generic32_t *) var_15 = var_16;
            segment_2.offset_20320 = segment_2.offset_20320 + 32;
            var_17 = &((union_418 *) var_16)[12].member_0.offset_4;
            if (var_16 < (uint32_t) -256) {
              generic32_t var_18;
              generic32_t var_19;
              var_18 = 0;
              var_19 = var_14;
              generic32_t var_20;
              do {
                var_20 = var_18;
                var_14->member_1.offset_0[var_20].offset_0.offset_4 = '\000';
                ((union_418 *) var_19)->member_0.offset_0.member_3 = 4294967295;
                *(generic8_t *) ((pointer_or_number32_t) &var_14->member_1.offset_0[var_20] + 5) = '\n';
                var_18 = var_20 + 1;
                var_19 = &((union_418 *) var_19)->member_0.offset_8;
              } while (*(generic32_t *) var_15 + 256 > (uint32_t) &var_14->member_1.offset_0[var_20 + 1]);
              var_16 = &var_14->member_1.offset_0[var_20 + 1];
              var_17 = 0;
            }
            if (!((int32_t) segment_2.offset_20320 < (int32_t) llvm_smin_i32(stack.offset_68->offset_0, 2048))) {
              var_8 = var_16;
              var_9 = var_17;
              var_10 = llvm_smin_i32(stack.offset_68->offset_0, 2048);
              break;
            }
            union_418 *var_21;
            generic32_t var_22;
            var_15 = var_15 + 4;
            *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 256;
            var_21 = function_0x404f00_Code_x86(var_22);
            var_14 = var_21;
            if (var_14) {
              continue;
            }
            var_13 = var_17;
            var_9 = var_13;
            var_10 = segment_2.offset_20320;
            var_8 = 0;
            break;
          }
        }
      }
      generic32_t var_23;
      generic32_t var_24;
      var_7 = var_9;
      var_24 = !var_10 ? 64 : 0;
      var_23 = lshift(var_10, 4294967272);
      var_6 = &stack;
      if (!(var_24 | (var_23 & 0x80))) {
        struct_363 *var_25;
        generic32_t var_26;
        generic32_t var_27;
        generic32_t var_28;
        generic32_t var_29;
        var_28 = (pointer_or_number32_t) &stack.offset_68->offset_4 + stack.offset_68->offset_0 * 1;
        var_29 = &stack.offset_68->offset_4;
        var_25 = &stack;
        var_26 = var_8;
        var_27 = 0;
        generic32_t var_30;
        generic32_t var_31;
        while (true) {
          generic32_t var_32;
          var_31 = var_25;
          var_32 = var_26;
          var_30 = 4294967295;
          if (*(generic32_t *) var_28 != (pointer_or_number32_t) -1) {
            var_32 = (var_26 & 0xFFFFFF00) | *(generic8_t *) var_29;
            var_30 = *(generic32_t *) var_28;
            var_31 = var_25;
            if ((*(generic8_t *) var_29 & 0x1)) {
              generic32_t var_33;
              var_33 = var_25;
              if (!(*(generic8_t *) var_29 & 0x8)) {
                generic32_t var_34;
                var_33 = (pointer_or_number32_t) var_25 - 4;
                var_31 = var_33;
                *(generic32_t *) var_31 = *(generic32_t *) var_28;
                var_34 = ((cabifunction_763 *) segment_3.offset_652)();
                var_30 = *(generic32_t *) var_28;
                var_32 = 0;
                if (!var_34) {
                  if (!((int32_t) (var_27 + 1) < (int32_t) var_10)) {
                    break;
                  }
                  var_28 = var_28 + 4;
                  var_29 = var_29 + 1;
                  var_27 = var_27 + 1;
                  continue;
                }
              }
              var_31 = var_33;
              var_32 = *(generic32_t *) var_28;
              var_30 = ((var_27 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + ((var_27 >> 3) & 0xFFFFFFC) * 1);
              *(generic32_t *) var_30 = var_32;
              *(generic8_t *) (var_30 + 4) = *(generic8_t *) var_29;
            }
          }
          if (!((int32_t) (var_27 + 1) < (int32_t) var_10)) {
            break;
          }
          var_28 = var_28 + 4;
          var_29 = var_29 + 1;
          var_27 = var_27 + 1;
        }
        var_6 = var_31;
        var_7 = var_30;
      }
    }
  }
  generic32_t var_35;
  generic32_t var_36;
  var_35 = var_6;
  var_36 = 0;
  generic8_t var_37;
  generic32_t var_38;
  do {
    generic8_t var_39;
    generic32_t var_40;
    var_40 = var_35;
    var_39 = '\200';
    if (segment_2.offset_20064->member_1.offset_0[var_36].offset_0.offset_0 == (pointer_or_number32_t) -1) {
      generic32_t var_41;
      generic32_t var_42;
      generic32_t var_43;
      segment_2.offset_20064->member_1.offset_0[var_36].offset_0.offset_4 = '\201';
      var_43 = var_36 + 1 == 2 ? 4294967285 : 4294967284;
      var_42 = !var_36 ? 4294967286 : var_43;
      var_40 = var_35 - 4;
      *(generic32_t *) var_40 = var_42;
      var_41 = ((rawfunction_157 *) segment_3.offset_648)(var_7);
      var_39 = '@';
      if (var_41 != (pointer_or_number32_t) -1) {
        generic32_t var_44;
        *(generic32_t *) (var_35 - 8) = var_41;
        var_44 = ((rawfunction_158 *) segment_3.offset_652)(var_7);
        var_39 = '@';
        var_40 = var_35 - 8;
        if (var_44) {
          bool var_45 = false;
          segment_2.offset_20064->member_1.offset_0[var_36].offset_0.offset_0 = var_41;
          var_38 = var_35 - 8;
          var_39 = '@';
          var_40 = var_35 - 8;
          switch ((number8_t) var_44) {
            case 2:
            case 3:
            {
              break;
            } break;
            default:
            {
              var_37 = var_36 < 2;
              var_36 = var_36 + 1;
              if (var_37) {
                continue;
              }
              var_45 = true;
              break;
            } break;
          }
          if (var_45){
            break;}
          if ((number8_t) var_44 == 3) {
            var_39 = '\010';
            var_40 = var_35 - 8;
          }
        }
      }
    }
    var_38 = var_40;
    segment_2.offset_20064->member_1.offset_0[var_36].offset_0.offset_4 = segment_2.offset_20064->member_1.offset_0[var_36].offset_0.offset_4 | var_39;
    var_37 = var_36 < 2;
    var_36 = var_36 + 1;
  } while (var_37);
  *(generic32_t *) (var_38 - 4) = segment_2.offset_20320;
  ((rawfunction_159 *) segment_3.offset_700)(var_7);
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404430_Code_x86(void) {
  struct_347 stack;
  generic32_t var_0;
  stack.offset_8 = 0;
  stack.offset_4 = 4096;
  stack.offset_0 = 1;
  var_0 = ((cabifunction_764 *) segment_3.offset_640)();
  segment_2.offset_20052 = var_0;
  if (var_0) {
    function_0x404fb0_Code_x86();
  }
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404474_Code_x86(struct_422 *argument_0, struct_520 *argument_1, generic32_t argument_2) {
  struct_365 stack;
  struct_422 *var_0;
  struct_520 *var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  stack.offset_8 = (pointer_or_number32_t) &stack.offset_28 + 4;
  if (!(*(generic32_t *) ((pointer_or_number32_t) var_0 + 4) & 0x6)) {
    stack.offset_24 = var_0;
    stack.offset_28 = var_2;
    *(struct_422 ***) ((pointer_or_number32_t) var_1 - 4) = &stack.offset_24;
    if (*(generic32_t *) ((pointer_or_number32_t) var_1 + 12) == (pointer_or_number32_t) -1) {
      revng_abort("A longjmp was taken");
    } else {
      generic32_t var_3;
      generic32_t var_4;
      generic32_t var_5;
      generic32_t var_6;
      var_4 = *(generic32_t *) ((pointer_or_number32_t) var_1 + 8);
      var_6 = (pointer_or_number32_t) &stack.offset_28 + 4;
      var_3 = *(generic32_t *) ((pointer_or_number32_t) var_1 + 12);
      var_5 = var_1;
      while (true) {
        generic32_t var_7;
        generic32_t var_8;
        generic32_t var_9;
        var_9 = var_5;
        var_8 = var_6;
        var_7 = var_3 * 12;
        if (*(generic32_t *) (var_7 + var_4 + 4)) {
          generic32_t var_10;
          stack.offset_4 = var_3;
          stack.offset_0 = var_6;
          var_10 = ((cabifunction_843 *) *(generic32_t *) (var_7 + var_4 + 4))();
          if (var_10) {
            generic32_t var_11;
            var_11 = lshift(var_10, 4294967272);
            if (!((var_11 & 0x80))) {
              generic32_t var_12;
              stack.offset_4 = stack.offset_0->offset_12;
              function_0x40379c_Code_x86(var_12);
            }
            break;
          }
          var_7 = (number32_t) stack.offset_4 * 12;
          var_8 = stack.offset_0;
          var_9 = stack.offset_0->offset_12;
        }
        var_4 = *(generic32_t *) (var_9 + 8);
        var_3 = *(generic32_t *) (var_7 + var_4);
        if (var_3 != (pointer_or_number32_t) -1) {
          continue;
        }
        break;
      }
      revng_abort("A longjmp was taken");
    }
  } else {
    generic32_t var_13;
    generic32_t var_14;
    stack.offset_4 = (pointer_or_number32_t) &stack.offset_28 + 4;
    stack.offset_0 = 4294967295;
    *(struct_520 **) ((pointer_or_number32_t) &stack - 4) = var_1;
    function_0x4037de_Code_x86((struct_502 *) var_13, var_14);
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
void function_0x404550_Code_x86(void) {
  generic32_t var_0;
  switch ((number32_t) segment_2.offset_9692) {
    case 1:
    {
      *(generic32_t *) (revng_undefined_local_sp() - 4) = 252;
      function_0x404590_Code_x86(var_0);
    } break;
    case 0:
    {
      if (segment_2.offset_9696 == 1) {
        *(generic32_t *) (revng_undefined_local_sp() - 4) = 252;
        function_0x404590_Code_x86(var_0);
      }
    } break;
  }
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404590_Code_x86(generic32_t argument_0) {
  struct_349 stack;
  uint64_t loop_state_var;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = &segment_2.offset_10976;
  var_2 = 0;
  generic32_t var_3;
  while (true) {
    generic32_t var_4;
    var_4 = var_2;
    var_3 = var_4;
    if (*(generic32_t *) var_1 != argument_0) {
      var_2 = var_4 + 1;
      var_3 = var_2;
      var_1 = var_1 + 8;
      if (!(var_4 > 16)) {
        continue;
      }
    }
    break;
  }
  generic32_t var_5;
  var_5 = var_3;
  if (*(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10976 + var_5 * 8) == argument_0) {
    generic32_t var_6;
    generic32_t var_7;
    generic8_t var_8;
    generic32_t var_9;
    generic32_t var_10;
    generic32_t var_11;
    generic8_t var_12;
    generic32_t var_13;
    generic32_t var_14;
    generic32_t var_15;
    generic8_t var_16;
    generic32_t var_17;
    generic32_t var_18;
    generic8_t var_19;
    generic8_t var_20;
    generic32_t var_21;
    generic8_t var_22;
    generic32_t var_23;
    generic32_t var_24;
    generic32_t var_25;
    generic32_t var_26;
    generic32_t var_27;
    generic32_t var_28;
    generic32_t var_29;
    generic32_t var_30;
    generic32_t var_31;
    generic32_t var_32;
    generic32_t var_33;
    generic32_t var_34;
    generic32_t var_35;
    generic32_t var_36;
    generic32_t var_37;
    generic32_t var_38;
    generic32_t var_39;
    generic32_t var_40;
    generic32_t var_41;
    generic32_t var_42;
    generic32_t var_43;
    generic32_t var_44;
    generic32_t var_45;
    generic32_t var_46;
    generic32_t var_47;
    generic32_t var_48;
    generic32_t var_49;
    generic32_t var_50;
    generic32_t var_51;
    generic32_t var_52;
    generic32_t var_53;
    generic32_t var_54;
    generic32_t var_55;
    generic32_t var_56;
    generic32_t var_57;
    uint8_t *var_58;
    generic32_t var_59;
    generic32_t var_60;
    generic32_t var_61;
    generic32_t var_62;
    generic32_t var_63;
    generic32_t var_64;
    generic32_t var_65;
    generic32_t var_66;
    generic32_t var_67;
    generic32_t var_68;
    generic32_t var_69;
    generic32_t var_70;
    generic32_t var_71;
    generic32_t var_72;
    generic32_t var_73;
    generic64_t var_74;
    generic32_t var_75;
    generic32_t var_76;
    generic32_t var_77;
    generic32_t var_78;
    generic32_t var_79;
    generic32_t var_80;
    generic32_t var_81;
    generic32_t var_82;
    generic32_t var_83;
    generic32_t var_84;
    generic64_t var_85;
    generic32_t var_86;
    generic32_t var_87;
    generic32_t var_88;
    generic32_t var_89;
    generic32_t var_90;
    generic32_t var_91;
    generic32_t var_92;
    generic32_t var_93;
    switch ((number32_t) segment_2.offset_9692) {
      case 1:
      {
        if (!segment_2.offset_20064) {
          stack.offset_20 = 4294967284;
          var_25 = ((cabifunction_766 *) segment_3.offset_648)();
          var_92 = var_25;
          var_93 = &stack.offset_20;
        } else {
          var_92 = segment_2.offset_20064->member_1.offset_16.offset_0;
          var_93 = (pointer_or_number32_t) &stack.offset_20 + 4;
          if (var_92 == (pointer_or_number32_t) -1) {
            stack.offset_20 = 4294967284;
            var_25 = ((cabifunction_766 *) segment_3.offset_648)();
            var_92 = var_25;
            var_93 = &stack.offset_20;
          }
        }
        var_91 = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10976 + 4 + var_5 * 8);
        *(generic32_t *) (var_93 - 4) = 0;
        *(generic32_t *) (var_93 - 8) = var_93 + 16;
        var_90 = 0;
        while (true) {
          var_89 = var_90;
          if (*(generic8_t *) var_91) {
            var_91 = var_91 + 1;
            var_22 = var_90 == (pointer_or_number32_t) -2;
            var_90 = var_90 + 1;
            var_89 = 4294967294;
            if (!(var_22)) {
              continue;
            }
          }
          break;
        }
        *(generic32_t *) (var_93 - 12) = var_89;
        *(generic32_t *) (var_93 - 16) = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10976 + 4 + var_5 * 8);
        *(generic32_t *) (var_93 - 20) = var_92;
        ((cabifunction_767 *) segment_3.offset_632)();
        revng_abort("A longjmp was taken");
      } break;
      case 0:
      {
        if (segment_2.offset_9696 == 1) {
          if (!segment_2.offset_20064) {
            stack.offset_20 = 4294967284;
            var_25 = ((cabifunction_766 *) segment_3.offset_648)();
            var_92 = var_25;
            var_93 = &stack.offset_20;
          } else {
            var_92 = segment_2.offset_20064->member_1.offset_16.offset_0;
            var_93 = (pointer_or_number32_t) &stack.offset_20 + 4;
            if (var_92 == (pointer_or_number32_t) -1) {
              stack.offset_20 = 4294967284;
              var_25 = ((cabifunction_766 *) segment_3.offset_648)();
              var_92 = var_25;
              var_93 = &stack.offset_20;
            }
          }
          var_91 = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10976 + 4 + var_5 * 8);
          *(generic32_t *) (var_93 - 4) = 0;
          *(generic32_t *) (var_93 - 8) = var_93 + 16;
          var_90 = 0;
          while (true) {
            var_89 = var_90;
            if (*(generic8_t *) var_91) {
              var_91 = var_91 + 1;
              var_22 = var_90 == (pointer_or_number32_t) -2;
              var_90 = var_90 + 1;
              var_89 = 4294967294;
              if (!(var_22)) {
                continue;
              }
            }
            break;
          }
          *(generic32_t *) (var_93 - 12) = var_89;
          *(generic32_t *) (var_93 - 16) = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10976 + 4 + var_5 * 8);
          *(generic32_t *) (var_93 - 20) = var_92;
          ((cabifunction_767 *) segment_3.offset_632)();
          revng_abort("A longjmp was taken");
        } else {
          if (argument_0 != 252) {
            stack.offset_20 = 260;
            stack.offset_16 = &stack.offset_196[2];
            stack.offset_12 = 0;
            var_24 = ((cabifunction_765 *) segment_3.offset_572)();
            if (!var_24) {
              var_87 = &stack.offset_192;
              var_85 = 0;
              var_86 = 0;
              var_88 = (generic32_t) 4224940;
              do {
                var_21 = (number32_t) var_85;
                *(generic32_t *) var_87 = *(generic32_t *) var_88;
                var_86 = var_86 + 1;
                var_88 = (var_85 << 2) + 4224944;
                var_85 = var_85 + 1;
                var_87 = &stack.offset_196[var_21];
              } while (var_86 != 5);
              stack.offset_196[var_21].member_2 = *(generic16_t *) var_88;
              stack.offset_196[4].member_1.offset_2 = *(generic8_t *) (generic32_t) 4224962;
            }
            var_83 = &stack.offset_192;
            var_82 = 0;
            var_84 = 4294967295;
            while (true) {
              if (!*(generic8_t *) var_83) {
                var_78 = &stack.offset_192;
                if (!(var_84 < (uint32_t) -60)) {
                  break;
                }
              } else {
                var_84 = var_84 - 1;
                var_83 = var_83 + 1;
                var_20 = var_82 == (pointer_or_number32_t) -2;
                var_82 = var_82 + 1;
                if (!(var_20)) {
                  continue;
                }
              }
              stack.offset_8 = 3;
              var_81 = &stack.offset_192;
              var_80 = 0;
              loop_state_var = 1;
              break;
            }
            if (loop_state_var == 1) {
              while (true) {
                var_79 = 4294967294 - var_80;
                if (*(generic8_t *) var_81) {
                  var_81 = var_81 + 1;
                  var_19 = var_80 == (pointer_or_number32_t) -2;
                  var_80 = var_80 + 1;
                  var_79 = 0;
                  if (!(var_19)) {
                    continue;
                  }
                }
                break;
              }
              stack.offset_4 = "...";
              stack.offset_0 = (pointer_or_number32_t) &stack.offset_36 + 96 + ~var_79 * 1;
              var_23 = function_0x4057b0_Code_x86((struct_523 *) var_26, (struct_527 *) var_27, var_28);
              var_78 = (pointer_or_number32_t) &stack.offset_36 + 96 + ~var_79 * 1;
            }
            var_18 = var_78;
            var_76 = &stack.offset_32;
            var_74 = 0;
            var_75 = 0;
            var_77 = (generic32_t) 4224908;
            do {
              var_17 = (number32_t) var_74;
              *(generic32_t *) var_76 = *(generic32_t *) var_77;
              var_75 = var_75 + 1;
              var_77 = (var_74 << 2) + 4224912;
              var_74 = var_74 + 1;
              var_76 = &stack.offset_36[var_17];
            } while (var_75 != 6);
            stack.offset_36[var_17].member_0 = *(generic16_t *) var_77;
            var_71 = 0;
            var_72 = 4294967295;
            var_73 = var_18;
            while (true) {
              var_70 = var_73;
              var_69 = 0;
              if (var_72) {
                var_69 = 4294967294 - var_71;
                var_70 = var_18 + 1 + var_71;
                var_16 = *(generic8_t *) var_73;
                var_73 = var_73 + 1;
                var_72 = var_72 - 1;
                var_71 = var_71 + 1;
                if (var_16) {
                  continue;
                }
              }
              break;
            }
            var_68 = &stack.offset_32;
            var_67 = 0;
            while (true) {
              var_15 = var_67;
              var_14 = var_68;
              if (*(generic8_t *) var_14) {
                var_68 = var_14 + 1;
                var_67 = var_15 + 1;
                if (var_15 != (pointer_or_number32_t) -2) {
                  continue;
                }
              }
              break;
            }
            var_62 = var_14;
            var_63 = var_70 - ~var_69;
            if (!(var_69 > (uint32_t) -5)) {
              var_64 = 0;
              var_65 = var_70 - ~var_69;
              var_66 = var_14;
              do {
                var_64 = var_64 + 1;
                *(generic32_t *) var_66 = *(generic32_t *) var_65;
                var_65 = var_65 + 4;
                var_66 = var_66 + 4;
              } while (~var_69 >> 2 != var_64);
              var_62 = (pointer_or_number32_t) &stack.offset_32 + (~var_69 & 0xFFFFFFFC) * 1 + var_15 * 1;
              var_63 = var_70 + var_69 + (~var_69 & 0xFFFFFFFC) + 1;
            }
            if ((~var_69 & 0x3)) {
              var_59 = ~var_69 & 0x3;
              var_60 = var_63;
              var_61 = var_62;
              do {
                *(generic8_t *) var_61 = *(generic8_t *) var_60;
                var_60 = var_60 + 1;
                var_61 = var_61 + 1;
                var_59 = var_59 - 1;
              } while (var_59 != 0);
            }
            var_58 = "\n\n";
            var_56 = 0;
            var_57 = 4294967295;
            while (true) {
              var_13 = var_56;
              var_55 = var_58;
              var_54 = 0;
              if (var_57) {
                var_54 = 4294967294 - var_13;
                var_57 = var_57 - 1;
                var_12 = !*var_58;
                var_56 = var_13 + 1;
                var_58 = &var_58[1];
                var_55 = &"\n"[var_13];
                if (!(var_12)) {
                  continue;
                }
              }
              break;
            }
            var_53 = &stack.offset_32;
            var_52 = 0;
            while (true) {
              var_11 = var_52;
              var_10 = var_53;
              if (*(generic8_t *) var_10) {
                var_53 = var_10 + 1;
                var_52 = var_11 + 1;
                if (var_11 != (pointer_or_number32_t) -2) {
                  continue;
                }
              }
              break;
            }
            var_47 = var_10;
            var_48 = var_55 - ~var_54;
            if (!(var_54 > (uint32_t) -5)) {
              var_49 = 0;
              var_50 = var_55 - ~var_54;
              var_51 = var_10;
              do {
                var_49 = var_49 + 1;
                *(generic32_t *) var_51 = *(generic32_t *) var_50;
                var_50 = var_50 + 4;
                var_51 = var_51 + 4;
              } while (~var_54 >> 2 != var_49);
              var_47 = (pointer_or_number32_t) &stack.offset_32 + (~var_54 & 0xFFFFFFFC) * 1 + var_11 * 1;
              var_48 = &((uint8_t *) var_55)[(~var_54 & 0xFFFFFFFC) + (var_54 + 1)];
            }
            if ((~var_54 & 0x3)) {
              var_44 = ~var_54 & 0x3;
              var_45 = var_48;
              var_46 = var_47;
              do {
                *(generic8_t *) var_46 = *(generic8_t *) var_45;
                var_45 = var_45 + 1;
                var_46 = var_46 + 1;
                var_44 = var_44 - 1;
              } while (var_44 != 0);
            }
            var_43 = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10976 + 4 + var_5 * 8);
            var_9 = var_43 + 1;
            var_41 = 0;
            var_42 = 4294967295;
            while (true) {
              var_40 = var_43;
              var_39 = 0;
              if (var_42) {
                var_39 = 4294967294 - var_41;
                var_40 = var_9 + var_41;
                var_8 = *(generic8_t *) var_43;
                var_43 = var_43 + 1;
                var_42 = var_42 - 1;
                var_41 = var_41 + 1;
                if (var_8) {
                  continue;
                }
              }
              break;
            }
            var_38 = &stack.offset_32;
            var_37 = 0;
            while (true) {
              var_7 = var_37;
              var_6 = var_38;
              if (*(generic8_t *) var_6) {
                var_38 = var_6 + 1;
                var_37 = var_7 + 1;
                if (var_7 != (pointer_or_number32_t) -2) {
                  continue;
                }
              }
              break;
            }
            var_32 = var_6;
            var_33 = var_40 - ~var_39;
            if (!(var_39 > (uint32_t) -5)) {
              var_34 = 0;
              var_35 = var_40 - ~var_39;
              var_36 = var_6;
              do {
                var_34 = var_34 + 1;
                *(generic32_t *) var_36 = *(generic32_t *) var_35;
                var_35 = var_35 + 4;
                var_36 = var_36 + 4;
              } while (~var_39 >> 2 != var_34);
              var_32 = (pointer_or_number32_t) &stack.offset_32 + (~var_39 & 0xFFFFFFFC) * 1 + var_7 * 1;
              var_33 = var_40 + var_39 + (~var_39 & 0xFFFFFFFC) + 1;
            }
            stack.offset_8 = 73744;
            stack.offset_4 = "Microsoft Visual C++ Runtime Library";
            if (!(~var_39 & 0x3)) {
              stack.offset_0 = &stack.offset_32;
              function_0x405710_Code_x86();
            }
            var_29 = var_33;
            var_30 = var_32;
            var_31 = ~var_39 & 0x3;
            do {
              *(generic8_t *) var_30 = *(generic8_t *) var_29;
              var_29 = var_29 + 1;
              var_30 = var_30 + 1;
              var_31 = var_31 - 1;
            } while (var_31 != 0);
            stack.offset_0 = &stack.offset_32;
            function_0x405710_Code_x86();
          }
          revng_abort("A longjmp was taken");
        }
      } break;
      default:
      {
        if (argument_0 != 252) {
          stack.offset_20 = 260;
          stack.offset_16 = &stack.offset_196[2];
          stack.offset_12 = 0;
          var_24 = ((cabifunction_765 *) segment_3.offset_572)();
          if (!var_24) {
            var_87 = &stack.offset_192;
            var_85 = 0;
            var_86 = 0;
            var_88 = (generic32_t) 4224940;
            do {
              var_21 = (number32_t) var_85;
              *(generic32_t *) var_87 = *(generic32_t *) var_88;
              var_86 = var_86 + 1;
              var_88 = (var_85 << 2) + 4224944;
              var_85 = var_85 + 1;
              var_87 = &stack.offset_196[var_21];
            } while (var_86 != 5);
            stack.offset_196[var_21].member_2 = *(generic16_t *) var_88;
            stack.offset_196[4].member_1.offset_2 = *(generic8_t *) (generic32_t) 4224962;
          }
          var_83 = &stack.offset_192;
          var_82 = 0;
          var_84 = 4294967295;
          while (true) {
            if (!*(generic8_t *) var_83) {
              var_78 = &stack.offset_192;
              if (!(var_84 < (uint32_t) -60)) {
                break;
              }
            } else {
              var_84 = var_84 - 1;
              var_83 = var_83 + 1;
              var_20 = var_82 == (pointer_or_number32_t) -2;
              var_82 = var_82 + 1;
              if (!(var_20)) {
                continue;
              }
            }
            stack.offset_8 = 3;
            var_81 = &stack.offset_192;
            var_80 = 0;
            loop_state_var = 1;
            break;
          }
          if (loop_state_var == 1) {
            while (true) {
              var_79 = 4294967294 - var_80;
              if (*(generic8_t *) var_81) {
                var_81 = var_81 + 1;
                var_19 = var_80 == (pointer_or_number32_t) -2;
                var_80 = var_80 + 1;
                var_79 = 0;
                if (!(var_19)) {
                  continue;
                }
              }
              break;
            }
            stack.offset_4 = "...";
            stack.offset_0 = (pointer_or_number32_t) &stack.offset_36 + 96 + ~var_79 * 1;
            var_23 = function_0x4057b0_Code_x86((struct_523 *) var_26, (struct_527 *) var_27, var_28);
            var_78 = (pointer_or_number32_t) &stack.offset_36 + 96 + ~var_79 * 1;
          }
          var_18 = var_78;
          var_76 = &stack.offset_32;
          var_74 = 0;
          var_75 = 0;
          var_77 = (generic32_t) 4224908;
          do {
            var_17 = (number32_t) var_74;
            *(generic32_t *) var_76 = *(generic32_t *) var_77;
            var_75 = var_75 + 1;
            var_77 = (var_74 << 2) + 4224912;
            var_74 = var_74 + 1;
            var_76 = &stack.offset_36[var_17];
          } while (var_75 != 6);
          stack.offset_36[var_17].member_0 = *(generic16_t *) var_77;
          var_71 = 0;
          var_72 = 4294967295;
          var_73 = var_18;
          while (true) {
            var_70 = var_73;
            var_69 = 0;
            if (var_72) {
              var_69 = 4294967294 - var_71;
              var_70 = var_18 + 1 + var_71;
              var_16 = *(generic8_t *) var_73;
              var_73 = var_73 + 1;
              var_72 = var_72 - 1;
              var_71 = var_71 + 1;
              if (var_16) {
                continue;
              }
            }
            break;
          }
          var_68 = &stack.offset_32;
          var_67 = 0;
          while (true) {
            var_15 = var_67;
            var_14 = var_68;
            if (*(generic8_t *) var_14) {
              var_68 = var_14 + 1;
              var_67 = var_15 + 1;
              if (var_15 != (pointer_or_number32_t) -2) {
                continue;
              }
            }
            break;
          }
          var_62 = var_14;
          var_63 = var_70 - ~var_69;
          if (!(var_69 > (uint32_t) -5)) {
            var_64 = 0;
            var_65 = var_70 - ~var_69;
            var_66 = var_14;
            do {
              var_64 = var_64 + 1;
              *(generic32_t *) var_66 = *(generic32_t *) var_65;
              var_65 = var_65 + 4;
              var_66 = var_66 + 4;
            } while (~var_69 >> 2 != var_64);
            var_62 = (pointer_or_number32_t) &stack.offset_32 + (~var_69 & 0xFFFFFFFC) * 1 + var_15 * 1;
            var_63 = var_70 + var_69 + (~var_69 & 0xFFFFFFFC) + 1;
          }
          if ((~var_69 & 0x3)) {
            var_59 = ~var_69 & 0x3;
            var_60 = var_63;
            var_61 = var_62;
            do {
              *(generic8_t *) var_61 = *(generic8_t *) var_60;
              var_60 = var_60 + 1;
              var_61 = var_61 + 1;
              var_59 = var_59 - 1;
            } while (var_59 != 0);
          }
          var_58 = "\n\n";
          var_56 = 0;
          var_57 = 4294967295;
          while (true) {
            var_13 = var_56;
            var_55 = var_58;
            var_54 = 0;
            if (var_57) {
              var_54 = 4294967294 - var_13;
              var_57 = var_57 - 1;
              var_12 = !*var_58;
              var_56 = var_13 + 1;
              var_58 = &var_58[1];
              var_55 = &"\n"[var_13];
              if (!(var_12)) {
                continue;
              }
            }
            break;
          }
          var_53 = &stack.offset_32;
          var_52 = 0;
          while (true) {
            var_11 = var_52;
            var_10 = var_53;
            if (*(generic8_t *) var_10) {
              var_53 = var_10 + 1;
              var_52 = var_11 + 1;
              if (var_11 != (pointer_or_number32_t) -2) {
                continue;
              }
            }
            break;
          }
          var_47 = var_10;
          var_48 = var_55 - ~var_54;
          if (!(var_54 > (uint32_t) -5)) {
            var_49 = 0;
            var_50 = var_55 - ~var_54;
            var_51 = var_10;
            do {
              var_49 = var_49 + 1;
              *(generic32_t *) var_51 = *(generic32_t *) var_50;
              var_50 = var_50 + 4;
              var_51 = var_51 + 4;
            } while (~var_54 >> 2 != var_49);
            var_47 = (pointer_or_number32_t) &stack.offset_32 + (~var_54 & 0xFFFFFFFC) * 1 + var_11 * 1;
            var_48 = &((uint8_t *) var_55)[(~var_54 & 0xFFFFFFFC) + (var_54 + 1)];
          }
          if ((~var_54 & 0x3)) {
            var_44 = ~var_54 & 0x3;
            var_45 = var_48;
            var_46 = var_47;
            do {
              *(generic8_t *) var_46 = *(generic8_t *) var_45;
              var_45 = var_45 + 1;
              var_46 = var_46 + 1;
              var_44 = var_44 - 1;
            } while (var_44 != 0);
          }
          var_43 = *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_10976 + 4 + var_5 * 8);
          var_9 = var_43 + 1;
          var_41 = 0;
          var_42 = 4294967295;
          while (true) {
            var_40 = var_43;
            var_39 = 0;
            if (var_42) {
              var_39 = 4294967294 - var_41;
              var_40 = var_9 + var_41;
              var_8 = *(generic8_t *) var_43;
              var_43 = var_43 + 1;
              var_42 = var_42 - 1;
              var_41 = var_41 + 1;
              if (var_8) {
                continue;
              }
            }
            break;
          }
          var_38 = &stack.offset_32;
          var_37 = 0;
          while (true) {
            var_7 = var_37;
            var_6 = var_38;
            if (*(generic8_t *) var_6) {
              var_38 = var_6 + 1;
              var_37 = var_7 + 1;
              if (var_7 != (pointer_or_number32_t) -2) {
                continue;
              }
            }
            break;
          }
          var_32 = var_6;
          var_33 = var_40 - ~var_39;
          if (!(var_39 > (uint32_t) -5)) {
            var_34 = 0;
            var_35 = var_40 - ~var_39;
            var_36 = var_6;
            do {
              var_34 = var_34 + 1;
              *(generic32_t *) var_36 = *(generic32_t *) var_35;
              var_35 = var_35 + 4;
              var_36 = var_36 + 4;
            } while (~var_39 >> 2 != var_34);
            var_32 = (pointer_or_number32_t) &stack.offset_32 + (~var_39 & 0xFFFFFFFC) * 1 + var_7 * 1;
            var_33 = var_40 + var_39 + (~var_39 & 0xFFFFFFFC) + 1;
          }
          stack.offset_8 = 73744;
          stack.offset_4 = "Microsoft Visual C++ Runtime Library";
          if (!(~var_39 & 0x3)) {
            stack.offset_0 = &stack.offset_32;
            function_0x405710_Code_x86();
          }
          var_29 = var_33;
          var_30 = var_32;
          var_31 = ~var_39 & 0x3;
          do {
            *(generic8_t *) var_30 = *(generic8_t *) var_29;
            var_29 = var_29 + 1;
            var_30 = var_30 + 1;
            var_31 = var_31 - 1;
          } while (var_31 != 0);
          stack.offset_0 = &stack.offset_32;
          function_0x405710_Code_x86();
        }
        revng_abort("A longjmp was taken");
      } break;
    }
  } else {
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x404790_Code_x86(generic32_t argument_0, generic8_t *argument_1, generic32_t argument_2) {
  struct_350 stack;
  generic32_t var_0;
  generic8_t *var_1;
  generic32_t var_2;
  generic32_t var_3;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  if (segment_2.offset_20320 > argument_0) {
    stack.offset_0.member_0.offset_44 = (pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1;
    stack.offset_0.member_0.offset_48 = (argument_0 << 3) & 0xF8;
    if ((*(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0x1)) {
      *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_48 + 4) = 0;
      var_3 = 0;
      if (!var_2) {
        return var_3;
      }
      if ((*(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0x20)) {
        generic32_t var_4;
        generic32_t var_5;
        generic32_t var_6;
        generic32_t var_7;
        stack.offset_0.member_0.offset_16 = 2;
        stack.offset_0.member_0.offset_12 = 0;
        stack.offset_0.member_0.offset_8 = argument_0;
        var_4 = function_0x4049c0_Code_x86(var_5, var_6, var_7);
      }
      generic32_t var_8;
      generic32_t var_9;
      generic32_t var_10;
      struct_578 **var_11;
      generic32_t var_12;
      generic32_t var_13;
      generic32_t var_14;
      var_8 = stack.offset_0.member_0.offset_48 + *stack.offset_0.member_0.offset_44;
      if ((int8_t) *(generic8_t *) (var_8 + 4) > -'\001') {
        generic32_t var_15;
        stack.offset_0.member_0.offset_16 = 0;
        stack.offset_0.member_0.offset_12 = (pointer_or_number32_t) &stack.offset_0.member_0.offset_32 + 8;
        stack.offset_0.member_0.offset_8 = var_2;
        stack.offset_0.member_0.offset_4 = var_1;
        stack.offset_0.member_0.offset_0 = *(generic32_t *) var_8;
        var_15 = ((cabifunction_769 *) segment_3.offset_632)();
        var_14 = &stack;
        if (!var_15) {
          var_13 = var_14;
          var_9 = ((cabifunction_770 *) segment_3.offset_628)();
          var_12 = var_9;
          var_11 = &((struct_350 *) var_13)->offset_0.member_0.offset_16;
        } else {
          stack.offset_0.member_0.offset_16 = 0;
          var_12 = stack.offset_0.member_0.offset_20;
          var_11 = &stack.offset_0.member_0.offset_32;
          var_13 = &stack;
        }
        var_10 = var_13;
        *var_11 = var_12;
      } else {
        generic32_t var_16;
        generic32_t var_17;
        generic32_t var_18;
        generic32_t var_19;
        *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_32 + 4) = 0;
        var_18 = var_1;
        var_17 = &stack.offset_0.member_0.offset_20;
        var_16 = 0;
        var_19 = 0;
        while (true) {
          generic32_t var_20;
          generic32_t var_21;
          generic32_t var_22;
          var_20 = var_17;
          var_22 = var_20;
          var_21 = (pointer_or_number32_t) &stack + var_16 * (number32_t) -20;
          if (var_18 - *(generic32_t *) ((pointer_or_number32_t) &(&stack)[1].offset_0.member_0.offset_8 + var_16 * (number32_t) -20 * 1) < var_2) {
            generic32_t var_23;
            generic32_t var_24;
            generic32_t var_25;
            generic32_t var_26;
            var_25 = (pointer_or_number32_t) &stack.offset_0.member_0.offset_48 + 8 + var_16 * (number32_t) -20 * 1;
            var_23 = 0;
            var_24 = var_18;
            var_26 = var_19;
            generic32_t var_27;
            while (true) {
              generic32_t var_28;
              generic32_t var_29;
              generic32_t var_30;
              var_28 = var_18 + 1 + var_23;
              if (!(var_24 - *(generic32_t *) ((pointer_or_number32_t) &(&stack)[1].offset_0.member_0.offset_8 + var_16 * (number32_t) -20 * 1) < var_2)) {
                var_27 = var_25 - (var_20 + 36);
                var_29 = var_24;
                var_30 = var_26;
                break;
              }
              generic32_t var_31;
              generic32_t var_32;
              generic32_t var_33;
              var_31 = var_25;
              var_32 = var_26;
              var_33 = var_25;
              if (*(generic8_t *) var_24 == '\n') {
                *(generic8_t *) var_25 = '\015';
                var_32 = var_26 + 1;
                var_33 = var_25 + 1;
                var_31 = var_33;
              }
              *(generic8_t *) var_31 = *(generic8_t *) var_24;
              var_25 = var_33 + 1;
              var_23 = var_23 + 1;
              var_24 = var_24 + 1;
              if ((int32_t) (var_25 - (var_20 + 36)) < (int32_t) 1024) {
                continue;
              }
              var_27 = var_25 - (var_20 + 36);
              var_29 = var_28;
              var_30 = var_32;
              break;
            }
            generic32_t var_34;
            *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_16 + var_16 * (number32_t) -20 * 1) = 0;
            *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_12 + var_16 * (number32_t) -20 * 1) = (pointer_or_number32_t) &stack.offset_0.member_0.offset_32 + 8 + var_16 * (number32_t) -20 * 1;
            *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_8 + var_16 * (number32_t) -20 * 1) = var_27;
            *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_4 + var_16 * (number32_t) -20 * 1) = (pointer_or_number32_t) &stack.offset_0.member_0.offset_48 + 8 + var_16 * (number32_t) -20 * 1;
            *(generic32_t *) var_21 = *(generic32_t *) (*(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_48 + var_16 * (number32_t) -20 * 1) + *(generic32_t *) *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_44 + var_16 * (number32_t) -20 * 1));
            var_34 = ((cabifunction_768 *) segment_3.offset_632)();
            if (!var_34) {
              var_14 = var_21;
              var_13 = var_14;
              var_9 = ((cabifunction_770 *) segment_3.offset_628)();
              var_12 = var_9;
              var_11 = &((struct_350 *) var_13)->offset_0.member_0.offset_16;
              var_10 = var_13;
              *var_11 = var_12;
              break;
            }
            var_17 = var_20 - 20;
            *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_32 + var_16 * (number32_t) -20 * 1) = *(generic32_t *) ((pointer_or_number32_t) &stack.offset_0.member_0.offset_32 + var_16 * (number32_t) -20 * 1) + *(generic32_t *) var_20;
            var_16 = var_16 + 1;
            var_22 = var_21;
            if (!((int32_t) *(generic32_t *) var_20 < (int32_t) var_27)) {
              continue;
            }
          }
          var_10 = var_22;
          break;
        }
      }
      if (!((struct_350 *) var_10)->offset_0.member_0.offset_32) {
        switch ((number32_t) ((struct_350 *) var_10)->offset_0.member_0.offset_16) {
          case 0:
          {
            break;
          } break;
          case 5:
          {
            segment_2.offset_9592 = 9;
            segment_2.offset_9596 = ((struct_350 *) var_10)->offset_0.member_0.offset_16;
            revng_abort("A longjmp was taken");
          } break;
          default:
          {
            *(struct_578 **) (var_10 - 4) = ((struct_350 *) var_10)->offset_0.member_0.offset_16;
            revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
            function_0x4058b0_Code_x86(0);
            revng_abort("A longjmp was taken");
          } break;
        }
        if (!(*(generic8_t *) (((struct_350 *) var_10)->offset_0.member_0.offset_28 + *(generic32_t *) *(generic32_t *) &((struct_350 *) var_10)->offset_0.member_0.offset_24 + 4) & 0x40)) {
          segment_2.offset_9592 = 28;
          segment_2.offset_9596 = 0;
          revng_abort("A longjmp was taken");
        } else {
          if (*((struct_350 *) var_10)->offset_0.member_0.offset_1072 == '\032') {
            revng_abort("A longjmp was taken");
          } else {
            segment_2.offset_9592 = 28;
            segment_2.offset_9596 = 0;
            revng_abort("A longjmp was taken");
          }
        }
      } else {
        revng_abort("A longjmp was taken");
      }
    }
  }
  segment_2.offset_9592 = 9;
  segment_2.offset_9596 = 0;
  var_3 = 4294967295;
  return var_3;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4049c0_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2) {
  struct_352 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  if ((segment_2.offset_20320 > argument_0) && ((*(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0x1))) {
    generic32_t var_3;
    generic32_t var_4;
    stack.offset_12 = argument_0;
    var_3 = function_0x4059b0_Code_x86(var_4);
    if (var_3 == (pointer_or_number32_t) -1) {
      segment_2.offset_9592 = 9;
      return 4294967295;
    }
    generic32_t var_5;
    stack.offset_12 = var_2;
    stack.offset_8 = 0;
    stack.offset_4 = var_1;
    stack.offset_0 = var_3;
    var_5 = ((cabifunction_771 *) segment_3.offset_624)();
    if (var_5 == (pointer_or_number32_t) -1) {
      generic32_t var_6;
      var_6 = ((cabifunction_772 *) segment_3.offset_628)();
      if (!var_6) {
        *(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) = *(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0xFD;
        revng_abort("A longjmp was taken");
      } else {
        generic32_t var_7;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = var_6;
        function_0x4058b0_Code_x86(var_7);
        revng_abort("A longjmp was taken");
      }
    } else {
      *(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) = *(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0xFD;
      revng_abort("A longjmp was taken");
    }
  }
  segment_2.offset_9592 = 9;
  segment_2.offset_9596 = 0;
  return 4294967295;
}

_ABI(Microsoft_x86_cdecl)
void function_0x404a80_Code_x86(struct_461 *argument_0) {
  struct_351 stack;
  union_418 *var_0;
  struct_461 *var_1;
  generic32_t var_2;
  generic32_t var_3;
  var_1 = argument_0;
  segment_2.offset_11768 = segment_2.offset_11768 + 1;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 4096;
  var_0 = function_0x404f00_Code_x86(var_2);
  argument_0->offset_8 = var_0;
  if (!var_0) {
    argument_0->offset_12 = argument_0->offset_12 | 0x4;
    argument_0->offset_8 = (pointer_or_number32_t) &argument_0->offset_12 + 8;
    var_3 = 2;
  } else {
    argument_0->offset_12 = argument_0->offset_12 | 0x8;
    var_3 = 4096;
  }
  argument_0->offset_24 = var_3;
  argument_0->offset_0 = argument_0->offset_8;
  argument_0->offset_4 = 0;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x404ad0_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  var_1 = 0;
  if (segment_2.offset_20320 > argument_0) {
    var_1 = *(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0x40;
  }
  return var_1;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404b00_Code_x86(void) {
  struct_367 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_2 = 512;
  if (segment_2.offset_20048) {
    var_2 = 20;
    if (!((int32_t) segment_2.offset_20048 < (int32_t) 20)) {
      *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 4;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = segment_2.offset_20048;
      function_0x405a00_Code_x86(var_0, var_1);
    }
  }
  segment_2.offset_20048 = var_2;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 4;
  *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = segment_2.offset_20048;
  function_0x405a00_Code_x86(var_0, var_1);
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x404bd0_Code_x86(void) {
  struct_368 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = function_0x405be0_Code_x86();
  var_1 = var_0;
  if (segment_2.offset_9656) {
    stack.offset_0 = 131;
    var_1 = 0;
    if ((int32_t) segment_2.offset_20048 > (int32_t) 3) {
      generic32_t var_2;
      generic32_t var_3;
      generic32_t var_4;
      var_2 = 0;
      var_3 = 12;
      var_4 = 0;
      generic8_t var_5;
      generic32_t var_6;
      do {
        var_6 = var_4;
        if (*(generic32_t *) (var_3 + (pointer_or_number32_t) segment_2.offset_15936)) {
          generic32_t var_7;
          var_7 = var_4;
          if ((*(generic32_t *) (*(generic32_t *) (var_3 + (pointer_or_number32_t) segment_2.offset_15936) + 12) & 0x83)) {
            generic32_t var_8;
            generic32_t var_9;
            *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = *(generic32_t *) (var_3 + (pointer_or_number32_t) segment_2.offset_15936);
            var_8 = function_0x405e10_Code_x86((struct_400 *) var_9);
            var_7 = var_4 + (var_8 != (pointer_or_number32_t) -1);
          }
          var_6 = var_7;
          if (!((int32_t) var_3 < (int32_t) 80)) {
            generic32_t var_10;
            *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = *(generic32_t *) (var_3 + (pointer_or_number32_t) segment_2.offset_15936);
            function_0x404eb0_Code_x86(var_10);
            *(generic32_t *) (var_3 + (pointer_or_number32_t) segment_2.offset_15936) = 0;
            var_6 = var_7;
          }
        }
        var_5 = (int32_t) (var_2 + 4) < (int32_t) segment_2.offset_20048;
        var_3 = var_3 + 4;
        var_2 = var_2 + 1;
      } while (var_5);
      var_1 = var_6;
    }
  }
  return var_1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x404bf0_Code_x86(generic8_t *argument_0, generic16_t argument_1) {
  struct_370 stack;
  generic8_t *var_0;
  generic16_t var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = 0;
  if (argument_0) {
    if (segment_2.offset_11824) {
      generic32_t var_3;
      stack.offset_32 = 0;
      stack.offset_28 = &stack.offset_32;
      stack.offset_24 = 0;
      stack.offset_20 = segment_2.offset_10236;
      stack.offset_16 = argument_0;
      stack.offset_12 = 1;
      stack.offset_8 = &(&stack)[1].offset_8;
      stack.offset_4 = 544;
      stack.offset_0 = segment_2.offset_11840;
      var_3 = ((cabifunction_773 *) segment_3.offset_724)();
      if ((var_3) && (!stack.offset_0)) {
        revng_abort("A longjmp was taken");
      }
      segment_2.offset_9592 = 42;
      revng_abort("A longjmp was taken");
    }
    if (argument_1 > 255) {
      segment_2.offset_9592 = 42;
      var_2 = 4294967295;
    } else {
      *argument_0 = (number8_t) argument_1;
      var_2 = 1;
    }
  }
  return var_2;
}

_ABI(Microsoft_x86_cdecl)
struct_681 function_0x404c90_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2, generic32_t argument_3) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  var_2 = argument_0;
  var_3 = argument_1;
  var_4 = argument_2;
  var_5 = argument_3;
  if (!argument_3) {
    var_7 = argument_1 / argument_2;
    var_6 = (number32_t) ((((number64_t) (argument_1 % argument_2) << 32) | var_2) / argument_2);
  } else {
    generic32_t var_8;
    generic32_t var_9;
    generic32_t var_10;
    generic32_t var_11;
    var_8 = var_2;
    var_9 = argument_2;
    var_10 = argument_3;
    var_11 = argument_1;
    generic32_t var_12;
    generic32_t var_13;
    generic32_t var_14;
    generic32_t var_15;
    generic32_t var_16;
    generic32_t var_17;
    generic32_t var_18;
    generic32_t var_19;
    generic32_t var_20;
    generic32_t var_21;
    do {
      var_13 = var_10;
      var_12 = var_11;
      var_10 = var_13 >> 1;
      var_21 = var_13 < 2 ? 64 : 0;
      var_20 = lshift(var_10, 4294967272);
      var_19 = lshift(var_13 ^ var_10, 4294967276);
      var_18 = helper_rcrl_wrapper(NULL, var_9, 1, (((llvm_ctpop_i32(var_10 & 0xFF) << 2) & 0x4) | (var_13 & 0x1) | var_21 | (var_20 & 0x80) | (var_19 & 0x800)) ^ 0x4, &var_1);
      var_9 = var_18;
      var_11 = var_12 >> 1;
      var_17 = var_12 < 2 ? 64 : 0;
      var_16 = lshift(var_11, 4294967272);
      var_15 = lshift(var_12 ^ var_11, 4294967276);
      var_14 = helper_rcrl_wrapper(NULL, var_8, 1, (((llvm_ctpop_i32(var_11 & 0xFF) << 2) & 0x4) | (var_12 & 0x1) | var_17 | (var_16 & 0x80) | (var_15 & 0x800)) ^ 0x4, &var_0);
      var_8 = var_14;
    } while (!(var_13 < 2));
    if (var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) < var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18)) {
      var_6 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) - 1;
      var_7 = 0;
    } else {
      if (var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) > var_3) {
        var_6 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) - 1;
        var_7 = 0;
      } else {
        var_6 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18);
        var_7 = 0;
        if (var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) - var_3 <= ~var_3 && var_2 < (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4)) {
          var_6 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) - 1;
          var_7 = 0;
        }
      }
    }
  }
  struct_681 var_22;
  var_22.offset_0 = var_6;
  var_22.offset_4 = var_7;
  return var_22;
}

_ABI(Microsoft_x86_cdecl)
struct_683 function_0x404d00_Code_x86(generic32_t argument_0, generic32_t argument_1, generic32_t argument_2, generic32_t argument_3) {
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  var_2 = argument_0;
  var_3 = argument_1;
  var_4 = argument_2;
  var_5 = argument_3;
  if (!argument_3) {
    var_6 = (number32_t) ((((number64_t) (argument_1 % argument_2) << 32) | var_2) % argument_2);
    var_7 = 0;
  } else {
    generic32_t var_8;
    generic32_t var_9;
    generic32_t var_10;
    generic32_t var_11;
    var_8 = var_2;
    var_9 = argument_2;
    var_10 = argument_3;
    var_11 = argument_1;
    generic32_t var_12;
    generic32_t var_13;
    generic32_t var_14;
    generic32_t var_15;
    generic32_t var_16;
    generic32_t var_17;
    generic32_t var_18;
    generic32_t var_19;
    generic32_t var_20;
    generic32_t var_21;
    do {
      var_13 = var_10;
      var_12 = var_11;
      var_10 = var_13 >> 1;
      var_21 = var_13 < 2 ? 64 : 0;
      var_20 = lshift(var_10, 4294967272);
      var_19 = lshift(var_13 ^ var_10, 4294967276);
      var_18 = helper_rcrl_wrapper(NULL, var_9, 1, (((llvm_ctpop_i32(var_10 & 0xFF) << 2) & 0x4) | (var_13 & 0x1) | var_21 | (var_20 & 0x80) | (var_19 & 0x800)) ^ 0x4, &var_1);
      var_9 = var_18;
      var_11 = var_12 >> 1;
      var_17 = var_12 < 2 ? 64 : 0;
      var_16 = lshift(var_11, 4294967272);
      var_15 = lshift(var_12 ^ var_11, 4294967276);
      var_14 = helper_rcrl_wrapper(NULL, var_8, 1, (((llvm_ctpop_i32(var_11 & 0xFF) << 2) & 0x4) | (var_12 & 0x1) | var_17 | (var_16 & 0x80) | (var_15 & 0x800)) ^ 0x4, &var_0);
      var_8 = var_14;
    } while (!(var_13 < 2));
    generic32_t var_22;
    generic32_t var_23;
    if (var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) < var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18)) {
      var_22 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) - var_4;
      var_23 = var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) - (var_5 + (var_22 > ~var_4));
    } else {
      if (var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) > var_3) {
        var_22 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) - var_4;
        var_23 = var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) - (var_5 + (var_22 > ~var_4));
      } else {
        var_22 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4);
        var_23 = var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32);
        if (var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) - var_3 <= ~var_3 && var_2 < (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4)) {
          var_22 = (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) - var_4;
          var_23 = var_5 * (number32_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18) + (number32_t) ((uint64_t) ((((number64_t) (var_12 >> 1) << 32) | var_14) / var_18 * var_4) >> 32) - (var_5 + (var_22 > ~var_4));
        }
      }
    }
    var_6 = 0 - (var_22 - var_2);
    var_7 = var_3 + (var_22 - var_2 > ~var_2) - var_23 + (pointer_or_number32_t) (var_22 != var_2);
  }
  struct_683 var_24;
  var_24.offset_0 = var_6;
  var_24.offset_4 = var_7;
  return var_24;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404d80_Code_x86(void) {
  struct_355 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  generic32_t var_5;
  generic32_t var_6;
  artificial_struct_returned_by_rawfunction_173 var_7;
  stack.offset_40 = 0;
  if (!segment_2.offset_11812) {
    generic32_t var_8;
    stack.offset_32 = (pointer_or_number32_t) &stack.offset_52 + 2;
    stack.offset_28 = 1;
    stack.offset_24 = (pointer_or_number32_t) &segment_2 + 9468;
    stack.offset_20 = 1;
    stack.offset_16 = 0;
    var_8 = ((cabifunction_774 *) segment_3.offset_664)();
    if (!var_8) {
      generic32_t var_9;
      stack.offset_12 = (pointer_or_number32_t) &stack.offset_32 + 2;
      stack.offset_8 = 1;
      stack.offset_4 = "";
      stack.offset_0 = 1;
      var_9 = ((cabifunction_775 *) segment_3.offset_696)();
      if (!var_9) {
        revng_abort("A longjmp was taken");
      } else {
        segment_2.offset_11812 = 1;
        var_3 = &stack;
        var_2 = ((struct_355 *) var_3)->offset_40;
        if (!var_2) {
          var_2 = segment_2.offset_11840;
        }
        *(generic32_t *) (var_3 - 4) = 0;
        *(generic32_t *) (var_3 - 8) = 0;
        *(generic32_t *) (var_3 - 12) = ((struct_355 *) var_3)->offset_32;
        *(generic32_t *) (var_3 - 16) = ((struct_355 *) var_3)->offset_28;
        *(generic32_t *) (var_3 - 20) = 9;
        *(generic32_t *) (var_3 - 24) = var_2;
        var_7 = ((rawfunction_173 *) segment_3.offset_568)();
        var_1 = var_7.register_eax;
        var_0 = var_7.register_ecx;
        if (var_1) {
          *(generic32_t *) (var_3 - 28) = var_1;
          *(generic32_t *) (var_3 - 32) = 2;
          revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
          function_0x405a00_Code_x86(0, 0);
        }
        *(generic32_t *) (var_3 - 28) = 0;
        revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
        function_0x404eb0_Code_x86(0);
        revng_abort("A longjmp was taken");
      }
    } else {
      segment_2.offset_11812 = 2;
      var_6 = &stack.offset_16;
      var_5 = segment_3.offset_664;
      var_4 = *(generic32_t *) (var_6 + 44);
      if (!var_4) {
        var_4 = segment_2.offset_11824;
        *(generic32_t *) (var_6 - 4) = *(generic32_t *) (var_6 + 36);
        *(generic32_t *) (var_6 - 8) = *(generic32_t *) (var_6 + 32);
        *(generic32_t *) (var_6 - 12) = *(generic32_t *) (var_6 + 28);
        *(generic32_t *) (var_6 - 16) = *(generic32_t *) (var_6 + 24);
        *(generic32_t *) (var_6 - 20) = var_4;
        ((cabifunction_776 *) var_5)();
        revng_abort("A longjmp was taken");
      } else {
        *(generic32_t *) (var_6 - 4) = *(generic32_t *) (var_6 + 36);
        *(generic32_t *) (var_6 - 8) = *(generic32_t *) (var_6 + 32);
        *(generic32_t *) (var_6 - 12) = *(generic32_t *) (var_6 + 28);
        *(generic32_t *) (var_6 - 16) = *(generic32_t *) (var_6 + 24);
        *(generic32_t *) (var_6 - 20) = var_4;
        ((cabifunction_776 *) var_5)();
        revng_abort("A longjmp was taken");
      }
    }
  } else {
    var_5 = segment_3.offset_664;
    segment_2.offset_11812 = segment_2.offset_11812;
    var_6 = (pointer_or_number32_t) &stack.offset_32 + 4;
    var_3 = (pointer_or_number32_t) &stack.offset_32 + 4;
    switch ((number32_t) segment_2.offset_11812) {
      case 2:
      {
        var_4 = *(generic32_t *) (var_6 + 44);
        if (!var_4) {
          var_4 = segment_2.offset_11824;
          *(generic32_t *) (var_6 - 4) = *(generic32_t *) (var_6 + 36);
          *(generic32_t *) (var_6 - 8) = *(generic32_t *) (var_6 + 32);
          *(generic32_t *) (var_6 - 12) = *(generic32_t *) (var_6 + 28);
          *(generic32_t *) (var_6 - 16) = *(generic32_t *) (var_6 + 24);
          *(generic32_t *) (var_6 - 20) = var_4;
          ((cabifunction_776 *) var_5)();
          revng_abort("A longjmp was taken");
        } else {
          *(generic32_t *) (var_6 - 4) = *(generic32_t *) (var_6 + 36);
          *(generic32_t *) (var_6 - 8) = *(generic32_t *) (var_6 + 32);
          *(generic32_t *) (var_6 - 12) = *(generic32_t *) (var_6 + 28);
          *(generic32_t *) (var_6 - 16) = *(generic32_t *) (var_6 + 24);
          *(generic32_t *) (var_6 - 20) = var_4;
          ((cabifunction_776 *) var_5)();
          revng_abort("A longjmp was taken");
        }
      } break;
      case 1:
      {
        var_2 = ((struct_355 *) var_3)->offset_40;
        if (!var_2) {
          var_2 = segment_2.offset_11840;
        }
        *(generic32_t *) (var_3 - 4) = 0;
        *(generic32_t *) (var_3 - 8) = 0;
        *(generic32_t *) (var_3 - 12) = ((struct_355 *) var_3)->offset_32;
        *(generic32_t *) (var_3 - 16) = ((struct_355 *) var_3)->offset_28;
        *(generic32_t *) (var_3 - 20) = 9;
        *(generic32_t *) (var_3 - 24) = var_2;
        var_7 = ((rawfunction_173 *) segment_3.offset_568)();
        var_1 = var_7.register_eax;
        var_0 = var_7.register_ecx;
        if (var_1) {
          *(generic32_t *) (var_3 - 28) = var_1;
          *(generic32_t *) (var_3 - 32) = 2;
          revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
          function_0x405a00_Code_x86(0, 0);
        }
        *(generic32_t *) (var_3 - 28) = 0;
        revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
        function_0x404eb0_Code_x86(0);
        revng_abort("A longjmp was taken");
      } break;
      default:
      {
        revng_abort("A longjmp was taken");
      } break;
    }
  }
}

_ABI(Microsoft_x86_cdecl)
void function_0x404eb0_Code_x86(generic32_t argument_0) {
  struct_359 stack;
  generic32_t var_0;
  var_0 = argument_0;
  if (!argument_0) {
    revng_abort("A longjmp was taken");
  } else {
    struct_693 var_1;
    generic32_t var_2;
    generic32_t var_3;
    generic32_t var_4;
    stack.offset_8 = &stack.offset_16;
    *(generic32_t **) &stack.offset_4 = &stack.offset_20;
    stack.offset_0 = argument_0;
    var_1 = function_0x405260_Code_x86(var_2, (struct_531 **) var_3, (generic32_t *) var_4);
    if (var_1.offset_0) {
      generic32_t var_5;
      generic32_t var_6;
      generic32_t var_7;
      generic32_t var_8;
      var_5 = stack.offset_20;
      stack.offset_8 = var_1.offset_0;
      *(generic32_t *) &stack.offset_4 = stack.offset_16;
      stack.offset_0 = var_5;
      function_0x4052c0_Code_x86((struct_408 *) var_6, var_7, (generic8_t *) var_8);
      return;
    }
    stack.offset_8 = argument_0;
    *(generic32_t *) &stack.offset_4 = 0;
    stack.offset_0 = segment_2.offset_20052;
    ((cabifunction_777 *) segment_3.offset_692)();
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
union_418 *function_0x404f00_Code_x86(generic32_t argument_0) {
  union_418 *var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  var_1 = argument_0;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = segment_2.offset_14320;
  *(generic32_t *) (revng_undefined_local_sp() - 8) = argument_0;
  var_0 = function_0x404f20_Code_x86(var_2, var_3);
  return var_0;
}

_ABI(Microsoft_x86_cdecl)
union_418 *function_0x404f20_Code_x86(generic32_t argument_0, generic32_t argument_1) {
  struct_372 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  var_1 = argument_1;
  if (!(argument_0 > (uint32_t) -32)) {
    generic32_t var_2;
    var_2 = !argument_0 ? 1 : argument_0;
    while (true) {
      if (!(var_2 > (uint32_t) -32)) {
        generic32_t var_3;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = var_2;
        function_0x404f70_Code_x86(var_3);
      }
      if (argument_1) {
        generic32_t var_4;
        struct_276 var_5;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = var_2;
        var_4 = function_0x405de0_Code_x86(var_5);
        if (var_4) {
          continue;
        }
      }
      break;
    }
  }
  return (union_418 *) NULL;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404f70_Code_x86(generic32_t argument_0) {
  struct_373 stack;
  generic32_t var_0;
  generic32_t *var_1;
  var_0 = argument_0;
  if (((argument_0 + 15) & 0xFFFFFFF0) > segment_2.offset_13924) {
    var_1 = &stack.offset_8;
  } else {
    struct_536 *var_2;
    generic32_t var_3;
    stack.offset_8 = (uint32_t) (argument_0 + 15) >> 4;
    var_2 = function_0x405310_Code_x86(var_3);
    var_1 = &stack.offset_8;
    if (var_2) {
      revng_abort("A longjmp was taken");
    }
  }
  *var_1 = (argument_0 + 15) & 0xFFFFFFF0;
  stack.offset_4 = 0;
  stack.offset_0 = segment_2.offset_20052;
  ((cabifunction_778 *) segment_3.offset_660)();
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x404fb0_Code_x86(void) {
  struct_364 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = &segment_2.offset_11848;
  var_1 = (pointer_or_number32_t) &stack.offset_8 + 4;
  if (segment_2.offset_11848.member_1.offset_2064) {
    generic32_t var_2;
    stack.offset_8 = 2068;
    stack.offset_4 = 0;
    stack.offset_0 = segment_2.offset_20052;
    var_2 = ((cabifunction_781 *) segment_3.offset_660)();
    var_0 = var_2;
    var_1 = &stack;
    if (!var_0) {
      revng_abort("A longjmp was taken");
    }
  }
  generic32_t var_3;
  generic32_t var_4;
  *(generic32_t *) (var_1 - 4) = 4;
  *(generic32_t *) (var_1 - 8) = 8192;
  *(generic32_t *) (var_1 - 12) = 4194304;
  var_4 = var_1 - 16;
  *(generic32_t *) var_4 = 0;
  var_3 = ((cabifunction_779 *) segment_3.offset_616)();
  if (var_3) {
    generic32_t var_5;
    *(generic32_t *) (var_1 - 20) = 4;
    *(generic32_t *) (var_1 - 24) = 4096;
    *(generic32_t *) (var_1 - 28) = 65536;
    *(generic32_t *) (var_1 - 32) = var_3;
    var_5 = ((cabifunction_780 *) segment_3.offset_616)();
    if (var_5) {
      if (var_0 == (pointer_or_number32_t) &segment_2.offset_11848) {
        if (!segment_2.offset_11848.member_0.offset_0) {
          segment_2.offset_11848.member_0.offset_0 = &segment_2.offset_11848;
        }
        if (!*(generic32_t *) &segment_2.offset_11848.member_0.offset_4) {
          *(union_597 **) &segment_2.offset_11848.member_0.offset_4 = &segment_2.offset_11848;
        }
      } else {
        *(union_597 **) var_0 = &segment_2.offset_11848;
        *(generic32_t *) (var_0 + 4) = *(generic32_t *) &segment_2.offset_11848.member_0.offset_4;
        *(generic32_t *) &segment_2.offset_11848.member_0.offset_4 = var_0;
        *(generic32_t *) *(generic32_t *) (var_0 + 4) = var_0;
      }
      generic32_t var_6;
      *(generic32_t *) (var_0 + 2064) = var_3;
      *(generic32_t *) (var_0 + 8) = 0;
      *(generic32_t *) (var_0 + 12) = 16;
      var_6 = 0;
      while (true) {
        if ((int32_t) var_6 < (int32_t) 16) {
          *(generic8_t *) (var_0 + 16 + var_6) = '\360';
          *(generic8_t *) (var_0 + 1040 + var_6) = '\361';
        } else {
          *(generic8_t *) (var_0 + 16 + var_6) = '\377';
          *(generic8_t *) (var_0 + 1040 + var_6) = '\361';
          if (!((int32_t) var_6 < (int32_t) 1023 || (int32_t) var_6 > (int32_t) 2147483646)) {
            break;
          }
        }
        var_6 = var_6 + 1;
      }
      generic32_t var_7;
      generic32_t var_8;
      var_7 = 0;
      var_8 = var_3;
      do {
        *(generic32_t *) var_8 = 0;
        var_8 = var_8 + 4;
        var_7 = var_7 + 1;
      } while (var_7 != 16384);
      if (*(generic32_t *) (var_0 + 2064) + 65536 > var_3) {
        generic32_t var_9;
        generic32_t var_10;
        var_9 = 0;
        var_10 = var_3;
        generic8_t var_11;
        do {
          *(generic32_t *) var_10 = var_3 + 8 + ((number32_t) var_9 << 12);
          *(generic32_t *) (var_3 + 4 + ((number32_t) var_9 << 12)) = 240;
          *(generic8_t *) (var_3 + 248 + ((number32_t) var_9 << 12)) = '\377';
          var_10 = var_10 + 4096;
          var_11 = *(generic32_t *) (var_0 + 2064) + 65536 > var_3 + 4096 + ((number32_t) var_9 << 12);
          var_9 = var_9 + 1;
        } while (var_11);
        revng_abort("A longjmp was taken");
      } else {
        revng_abort("A longjmp was taken");
      }
    }
    *(generic32_t *) (var_1 - 36) = 32768;
    *(generic32_t *) (var_1 - 40) = 0;
    var_4 = var_1 - 44;
    *(generic32_t *) var_4 = var_3;
    ((cabifunction_782 *) segment_3.offset_636)();
  }
  if (var_0 == (pointer_or_number32_t) &segment_2.offset_11848) {
    revng_abort("A longjmp was taken");
  } else {
    *(generic32_t *) (var_4 - 4) = var_0;
    *(generic32_t *) (var_4 - 8) = 0;
    *(generic32_t *) (var_4 - 12) = segment_2.offset_20052;
    ((cabifunction_783 *) segment_3.offset_692)();
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405120_Code_x86(struct_412 *argument_0) {
  struct_376 stack;
  struct_412 *var_0;
  var_0 = argument_0;
  stack.offset_20 = 32768;
  stack.offset_16 = 0;
  stack.offset_12 = *(generic32_t *) ((pointer_or_number32_t) var_0 + 2064);
  ((cabifunction_784 *) segment_3.offset_636)();
  if ((pointer_or_number32_t) segment_2.offset_13916 == (pointer_or_number32_t) var_0) {
    segment_2.offset_13916 = *(generic32_t *) ((pointer_or_number32_t) var_0 + 4);
  }
  if ((pointer_or_number32_t) var_0 == (pointer_or_number32_t) &segment_2.offset_11848) {
    segment_2.offset_11848.member_1.offset_2064 = 0;
    revng_abort("A longjmp was taken");
  } else {
    stack.offset_8 = var_0;
    stack.offset_4 = 0;
    *(generic32_t *) *(generic32_t *) ((pointer_or_number32_t) var_0 + 4) = *(generic32_t *) var_0;
    *(generic32_t *) (*(generic32_t *) var_0 + 4) = *(generic32_t *) ((pointer_or_number32_t) var_0 + 4);
    stack.offset_0 = segment_2.offset_20052;
    ((cabifunction_785 *) segment_3.offset_692)();
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405180_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  var_1 = *(generic32_t *) &segment_2.offset_11848.member_0.offset_4;
  var_0 = revng_undefined_local_sp() - 20;
  while (true) {
    generic32_t var_2;
    generic32_t var_3;
    var_2 = var_0;
    var_3 = var_1;
    if (*(generic32_t *) (var_3 + 2064)) {
      generic32_t var_4;
      generic32_t var_5;
      generic32_t var_6;
      generic32_t var_7;
      generic32_t var_8;
      var_7 = var_1 + 1039;
      *(generic32_t *) (var_0 + 16) = 0;
      var_4 = 0;
      var_5 = var_0;
      var_6 = 1023;
      var_8 = 4190208;
      generic32_t var_9;
      while (true) {
        generic32_t var_10;
        var_10 = var_5;
        if (*(generic8_t *) var_7 == (pointer_or_number8_t) -'\020') {
          generic32_t var_11;
          *(generic32_t *) (var_5 - 4) = 16384;
          *(generic32_t *) (var_5 - 8) = 4096;
          var_10 = var_5 - 12;
          *(generic32_t *) var_10 = *(generic32_t *) (var_3 + 2064) + var_8;
          var_11 = ((cabifunction_786 *) segment_3.offset_636)();
          if (var_11) {
            *(generic8_t *) var_7 = '\377';
            segment_2.offset_13920 = segment_2.offset_13920 - 1;
            if (*(generic32_t *) (var_1 + 12) == (pointer_or_number32_t) -1 || (int32_t) var_6 < (int32_t) *(generic32_t *) (var_1 + 12)) {
              *(generic32_t *) (var_1 + 12) = var_6;
            }
            *(generic32_t *) (var_5 + 4) = *(generic32_t *) (var_5 + 4) + 1;
            *(generic32_t *) (var_5 + 12) = *(generic32_t *) (var_5 + 12) - 1;
            var_9 = var_5 - 12;
            var_10 = var_5 - 12;
            if (*(generic32_t *) (var_5 + 12) == 1) {
              break;
            }
          }
        }
        generic32_t var_12;
        var_9 = var_10;
        var_12 = lshift(4186112 - ((number32_t) var_4 << 12), 4294967272);
        if ((var_12 & 0x80)) {
          break;
        }
        var_8 = var_8 - 4096;
        var_7 = var_7 - 1;
        var_6 = var_6 - 1;
        var_4 = var_4 + 1;
      }
      var_2 = var_9;
      var_3 = *(generic32_t *) (var_1 + 4);
      if (*(generic32_t *) (var_2 + 16)) {
        var_2 = var_9;
        var_3 = *(generic32_t *) (var_1 + 4);
        if (*(generic8_t *) (var_1 + 16) == (pointer_or_number8_t) -'\001') {
          generic32_t var_13;
          var_13 = 1;
          if (*(generic8_t *) (var_1 + 17) == (pointer_or_number8_t) -'\001') {
            generic32_t var_14;
            var_14 = 0;
            generic32_t var_15;
            while (true) {
              var_15 = var_14 + 2;
              if (var_14 < 1022) {
                generic8_t var_16;
                var_16 = *(generic8_t *) (var_1 + 18 + var_14) == (pointer_or_number8_t) -'\001';
                var_14 = var_14 + 1;
                if (var_16) {
                  continue;
                }
              }
              break;
            }
            var_13 = var_15;
          }
          var_2 = var_9;
          var_3 = *(generic32_t *) (var_1 + 4);
          if (var_13 == 1024) {
            *(generic32_t *) (var_9 - 4) = var_1;
            revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
            function_0x405120_Code_x86((struct_412 *) NULL);
          }
        }
      }
    }
    if (var_3 == *(generic32_t *) &segment_2.offset_11848.member_0.offset_4) {
      revng_abort("A longjmp was taken");
    } else {
      if ((int32_t) *(generic32_t *) (var_2 + 24) > (int32_t) 0) {
        continue;
      }
      revng_abort("A longjmp was taken");
    }
  }
}

_ABI(Microsoft_x86_cdecl)
struct_693 function_0x405260_Code_x86(generic32_t argument_0, struct_531 **argument_1, generic32_t *argument_2) {
  generic32_t var_0;
  struct_531 **var_1;
  generic32_t *var_2;
  generic32_t var_3;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  var_3 = &segment_2.offset_11848;
  generic32_t var_4;
  generic32_t var_5;
  while (true) {
    if (*(generic32_t *) (var_3 + 2064) != 0 && *(generic32_t *) (var_3 + 2064) < argument_0 && *(generic32_t *) (var_3 + 2064) + 4194304 > argument_0) {
      *var_1 = var_3;
      *var_2 = argument_0 & 0xFFFFF000;
      var_5 = (int32_t) ((argument_0 & 0xFF0) - 256) >> 4;
      var_4 = (argument_0 & 0xFFFFF000) + var_5 + 8;
    } else {
      var_3 = *(generic32_t *) var_3;
      if (var_3 != (pointer_or_number32_t) &segment_2.offset_11848) {
        continue;
      }
      var_4 = 0;
      var_5 = argument_0;
    }
    break;
  }
  struct_693 var_6;
  var_6.offset_0 = var_4;
  var_6.offset_4 = var_5;
  return var_6;
}

_ABI(Microsoft_x86_cdecl)
void function_0x4052c0_Code_x86(struct_408 *argument_0, generic32_t argument_1, generic8_t *argument_2) {
  struct_371 stack;
  struct_408 *var_0;
  generic32_t var_1;
  generic8_t *var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  *(generic8_t *) (((int32_t) (argument_1 - argument_0->offset_2064) >> 12) + (pointer_or_number32_t) argument_0 + 16) = *argument_2 + *(generic8_t *) (((int32_t) (argument_1 - argument_0->offset_2064) >> 12) + (pointer_or_number32_t) argument_0 + 16);
  *argument_2 = '\000';
  *(generic8_t *) (((int32_t) (argument_1 - argument_0->offset_2064) >> 12) + (pointer_or_number32_t) argument_0 + 1040) = '\361';
  if (*(generic8_t *) (((int32_t) (argument_1 - argument_0->offset_2064) >> 12) + (pointer_or_number32_t) argument_0 + 16) == (pointer_or_number8_t) -'\020') {
    segment_2.offset_13920 = segment_2.offset_13920 + 1;
    if (segment_2.offset_13920 == 31) {
      *(generic32_t *) &stack = 16;
      function_0x405180_Code_x86();
    }
  }
}

_ABI(Microsoft_x86_cdecl)
struct_536 *function_0x405310_Code_x86(generic32_t argument_0) {
  struct_375 stack;
  uint64_t loop_state_var;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  var_1 = segment_2.offset_13916;
  generic32_t var_2;
  while (true) {
    generic32_t var_3;
    generic32_t var_4;
    var_3 = var_4;
    if (*(generic32_t *) (var_1 + 2064)) {
      generic32_t var_5;
      generic32_t var_6;
      struct_536 *var_7;
      generic32_t var_8;
      var_8 = var_4;
      if ((int32_t) *(generic32_t *) (var_1 + 8) < (int32_t) 1024) {
        generic32_t var_9;
        generic32_t var_10;
        generic32_t var_11;
        generic32_t var_12;
        generic32_t var_13;
        var_11 = (number32_t) *(generic32_t *) (var_1 + 8) << 12;
        var_9 = var_11 + 4096;
        var_10 = 0;
        var_12 = var_4;
        var_13 = *(generic32_t *) (var_1 + 8);
        generic32_t var_14;
        while (true) {
          var_14 = (var_12 & 0xFFFFFF00) | *(generic8_t *) (var_1 + 16 + *(generic32_t *) (var_1 + 8) + var_10);
          if (!(*(generic8_t *) (var_1 + 16 + *(generic32_t *) (var_1 + 8) + var_10) < argument_0 || *(generic8_t *) (var_1 + 16 + *(generic32_t *) (var_1 + 8) + var_10) == (pointer_or_number8_t) -'\001')) {
            var_14 = *(generic8_t *) (var_1 + 1040 + *(generic32_t *) (var_1 + 8) + var_10);
            if (var_14 > argument_0) {
              struct_536 *var_15;
              generic32_t var_16;
              generic32_t var_17;
              generic32_t var_18;
              stack.offset_12 = argument_0;
              stack.offset_8 = *(generic8_t *) (var_1 + 16 + *(generic32_t *) (var_1 + 8) + var_10);
              stack.offset_4 = *(generic32_t *) (var_1 + 2064) + var_11;
              var_15 = function_0x405590_Code_x86((struct_416 *) var_16, var_17, var_18);
              if (var_15) {
                var_5 = var_1 + 16 + *(generic32_t *) (var_1 + 8) + var_10;
                var_6 = var_13;
                var_7 = var_15;
                segment_2.offset_13916 = var_1;
                *(generic8_t *) var_5 = *(generic8_t *) var_5 - (number8_t) argument_0;
                *(generic32_t *) (var_1 + 8) = var_6;
                return var_7;
              }
              *(generic8_t *) (var_1 + 1040 + *(generic32_t *) (var_1 + 8) + var_10) = (number8_t) argument_0;
              var_14 = *(generic8_t *) (var_1 + 1040 + *(generic32_t *) (var_1 + 8) + var_10);
            }
          }
          if (!((int32_t) (var_9 + ((number32_t) var_10 << 12)) < (int32_t) 4194304)) {
            break;
          }
          var_11 = var_11 + 4096;
          var_13 = var_13 + 1;
          var_10 = var_10 + 1;
        }
        var_8 = var_14;
      }
      var_3 = var_8;
      if ((int32_t) *(generic32_t *) (var_1 + 8) > (int32_t) 0) {
        generic32_t var_19;
        generic32_t var_20;
        generic32_t var_21;
        var_19 = 0;
        var_20 = var_8;
        var_21 = 0;
        generic8_t var_22;
        generic32_t var_23;
        do {
          var_23 = (var_20 & 0xFFFFFF00) | *(generic8_t *) (var_1 + 16 + var_21);
          if (!(*(generic8_t *) (var_1 + 16 + var_21) < argument_0 || *(generic8_t *) (var_1 + 16 + var_21) == (pointer_or_number8_t) -'\001')) {
            var_23 = *(generic8_t *) (var_1 + 1040 + var_21);
            if (var_23 > argument_0) {
              struct_536 *var_24;
              generic32_t var_25;
              generic32_t var_26;
              generic32_t var_27;
              stack.offset_12 = argument_0;
              stack.offset_8 = *(generic8_t *) (var_1 + 16 + var_21);
              stack.offset_4 = *(generic32_t *) (var_1 + 2064) + var_19;
              var_24 = function_0x405590_Code_x86((struct_416 *) var_25, var_26, var_27);
              if (var_24) {
                var_5 = var_1 + 16 + var_21;
                var_6 = var_21;
                var_7 = var_24;
                segment_2.offset_13916 = var_1;
                *(generic8_t *) var_5 = *(generic8_t *) var_5 - (number8_t) argument_0;
                *(generic32_t *) (var_1 + 8) = var_6;
                return var_7;
              }
              *(generic8_t *) (var_1 + 1040 + var_21) = (number8_t) argument_0;
              var_23 = *(generic8_t *) (var_1 + 1040 + var_21);
            }
          }
          var_22 = (int32_t) *(generic32_t *) (var_1 + 8) > (int32_t) (var_21 + 1);
          var_19 = var_19 + 4096;
          var_21 = var_21 + 1;
        } while (var_22);
        var_3 = var_23;
      }
    }
    var_1 = *(generic32_t *) var_1;
    if ((pointer_or_number32_t) segment_2.offset_13916 != var_1) {
      continue;
    }
    var_2 = &segment_2.offset_11848;
    break;
  }
  generic32_t var_28;
  generic32_t var_29;
  generic32_t var_30;
  while (true) {
    if (*(generic32_t *) (var_2 + 2064)) {
      if (*(generic32_t *) (var_2 + 12) != (pointer_or_number32_t) -1) {
        var_28 = *(generic32_t *) (var_2 + 12) + 1;
        if (!((int32_t) llvm_smin_i32(*(generic32_t *) (var_2 + 12) + 16, 1024) > (int32_t) var_28)) {
          break;
        }
        var_29 = 0;
        var_30 = *(generic32_t *) (var_2 + 12) + 1;
        loop_state_var = 1;
        break;
      }
    }
    var_2 = *(generic32_t *) var_2;
    if (!(var_2 != (pointer_or_number32_t) &segment_2.offset_11848)) {
      function_0x404fb0_Code_x86();
    }
  }
  if (loop_state_var == 1) {
    generic32_t var_31;
    while (true) {
      var_31 = var_30;
      if (*(generic8_t *) (var_2 + 17 + *(generic32_t *) (var_2 + 12) + var_29) == (pointer_or_number8_t) -'\001') {
        var_31 = *(generic32_t *) (var_2 + 12) + 2 + var_29;
        var_30 = var_30 + 1;
        var_29 = var_29 + 1;
        if ((int32_t) llvm_smin_i32(*(generic32_t *) (var_2 + 12) + 16, 1024) > (int32_t) var_31) {
          continue;
        }
      }
      break;
    }
    var_28 = var_31;
  }
  generic32_t var_32;
  stack.offset_12 = 4;
  stack.offset_8 = 4096;
  stack.offset_4 = (number32_t) (var_28 - *(generic32_t *) (var_2 + 12)) << 12;
  stack.offset_0 = *(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12);
  var_32 = ((cabifunction_787 *) segment_3.offset_616)();
  if (var_32 == ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + *(generic32_t *) (var_2 + 2064)) {
    if ((int32_t) *(generic32_t *) (var_2 + 12) < (int32_t) var_28) {
      generic32_t var_33;
      generic32_t var_34;
      generic32_t var_35;
      generic32_t var_36;
      generic32_t var_37;
      var_37 = ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + *(generic32_t *) (var_2 + 2064);
      var_35 = var_37 + 248;
      var_34 = var_37 + 4;
      var_33 = var_37 + 8;
      var_36 = 0;
      generic8_t var_38;
      do {
        *(generic32_t *) var_37 = var_33 + ((number32_t) var_36 << 12);
        *(generic32_t *) (var_34 + ((number32_t) var_36 << 12)) = 240;
        *(generic8_t *) (var_35 + ((number32_t) var_36 << 12)) = '\377';
        *(generic8_t *) (*(generic32_t *) (var_2 + 12) + 16 + var_2 + var_36) = '\360';
        *(generic8_t *) (*(generic32_t *) (var_2 + 12) + 1040 + var_2 + var_36) = '\361';
        var_38 = (int32_t) (*(generic32_t *) (var_2 + 12) + 1 + var_36) < (int32_t) var_28;
        var_37 = var_37 + 4096;
        var_36 = var_36 + 1;
      } while (var_38);
    }
    generic32_t var_39;
    generic32_t var_40;
    segment_2.offset_13916 = var_2;
    var_40 = var_28;
    if ((int32_t) var_28 < (int32_t) 1024) {
      generic32_t var_41;
      generic32_t var_42;
      var_41 = 0;
      var_42 = var_28;
      generic32_t var_43;
      while (true) {
        var_43 = var_42;
        if (*(generic8_t *) (var_28 + 16 + var_2 + var_41) != (pointer_or_number8_t) -'\001') {
          var_43 = var_28 + 1 + var_41;
          var_42 = var_42 + 1;
          var_41 = var_41 + 1;
          if ((int32_t) var_43 < (int32_t) 1024) {
            continue;
          }
        }
        break;
      }
      var_40 = var_43;
      var_39 = (int32_t) var_40 < (int32_t) 1024 ? var_40 : 4294967295;
      *(generic32_t *) (var_2 + 12) = var_39;
      *(generic8_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + 8) = (number8_t) argument_0;
      *(generic32_t *) (var_2 + 8) = *(generic32_t *) (var_2 + 12);
      *(generic8_t *) (*(generic32_t *) (var_2 + 12) + var_2 + 16) = *(generic8_t *) (*(generic32_t *) (var_2 + 12) + var_2 + 16) - (number8_t) argument_0;
      *(generic32_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12)) = *(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + argument_0 + 8;
      *(generic32_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + 4) = *(generic32_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + 4) - argument_0;
      revng_abort("A longjmp was taken");
    } else {
      var_39 = (int32_t) var_40 < (int32_t) 1024 ? var_40 : 4294967295;
      *(generic32_t *) (var_2 + 12) = var_39;
      *(generic8_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + 8) = (number8_t) argument_0;
      *(generic32_t *) (var_2 + 8) = *(generic32_t *) (var_2 + 12);
      *(generic8_t *) (*(generic32_t *) (var_2 + 12) + var_2 + 16) = *(generic8_t *) (*(generic32_t *) (var_2 + 12) + var_2 + 16) - (number8_t) argument_0;
      *(generic32_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12)) = *(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + argument_0 + 8;
      *(generic32_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + 4) = *(generic32_t *) (*(generic32_t *) (var_2 + 2064) + ((number32_t) *(generic32_t *) (var_2 + 12) << 12) + 4) - argument_0;
      revng_abort("A longjmp was taken");
    }
  } else {
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
struct_536 *function_0x405590_Code_x86(struct_416 *argument_0, generic32_t argument_1, generic32_t argument_2) {
  uint64_t loop_state_var;
  struct_416 *var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  if ((uint32_t) argument_0->offset_4 < argument_2) {
    generic32_t var_4;
    generic32_t var_5;
    var_4 = !*(generic8_t *) ((pointer_or_number32_t) argument_0->offset_4 + (pointer_or_number32_t) argument_0->offset_0) ? (generic32_t) argument_0->offset_0 : (pointer_or_number32_t) argument_0->offset_4 + (pointer_or_number32_t) argument_0->offset_0;
    var_5 = var_1;
    if (var_4 + argument_2 - (number32_t) &argument_0[31] > 4294967047 - (number32_t) argument_0) {
      generic32_t var_6;
      generic32_t var_7;
      var_6 = var_4;
      var_7 = var_1;
      while (true) {
        generic32_t var_8;
        generic32_t var_9;
        if (!*(generic8_t *) var_6) {
          generic32_t var_10;
          generic32_t var_11;
          var_11 = var_6 + 1;
          var_10 = 1;
          if (!*(generic8_t *) var_11) {
            generic32_t var_12;
            var_12 = 0;
            generic32_t var_13;
            do {
              var_13 = var_12;
              var_12 = var_13 + 1;
            } while (!*(generic8_t *) (var_6 + 2 + var_13));
            var_10 = var_13 + 2;
            var_11 = var_6 + 2 + var_13;
          }
          if (!(var_10 < argument_2)) {
            if (var_6 + argument_2 < (uint32_t) &argument_0[31]) {
              argument_0->offset_0 = var_6 + argument_2;
              argument_0->offset_4 = var_10 - argument_2;
            } else {
              argument_0->offset_4 = 0;
              argument_0->offset_0 = &argument_0[1];
            }
            *(generic8_t *) var_6 = (number8_t) argument_2;
            var_3 = &argument_0[2 * (var_6 - (number32_t) argument_0) + 16];
            loop_state_var = 0;
            break;
          }
          if ((pointer_or_number32_t) argument_0->offset_0 == var_6) {
            argument_0->offset_4 = var_10;
            var_8 = var_11;
            var_9 = var_7;
          } else {
            var_9 = var_7 - var_10;
            var_8 = var_11;
            if (var_9 < argument_2) {
              var_3 = 0;
              loop_state_var = 0;
              break;
            }
          }
        } else {
          var_8 = var_6 + *(generic8_t *) var_6;
          var_9 = var_7;
        }
        if (var_8 + argument_2 < (uint32_t) &argument_0[31]) {
          continue;
        }
        var_5 = var_9;
        break;
      }
      if (!(loop_state_var)) {
        return (struct_536 *) var_3;
      }
    }
    var_3 = 0;
    if ((uint32_t) argument_0->offset_0 > (uint32_t) &argument_0[1]) {
      struct_416 *var_14;
      generic32_t var_15;
      var_14 = &argument_0[1];
      var_15 = var_5;
      while (true) {
        if (!((pointer_or_number32_t) var_14 + argument_2 > (pointer_or_number32_t) &argument_0[30].offset_4 + 3)) {
          generic32_t var_16;
          generic32_t var_17;
          if (!*(generic8_t *) var_14) {
            generic32_t var_18;
            generic32_t var_19;
            var_19 = (pointer_or_number32_t) var_14 + 1;
            var_18 = 1;
            if (!*(generic8_t *) var_19) {
              generic32_t var_20;
              var_20 = 0;
              generic32_t var_21;
              do {
                var_21 = var_20;
                var_20 = var_21 + 1;
              } while (!*(generic8_t *) ((pointer_or_number32_t) var_14 + 2 + var_21));
              var_18 = var_21 + 2;
              var_19 = (pointer_or_number32_t) var_14 + 2 + var_21;
            }
            if (!(var_18 < argument_2)) {
              if ((pointer_or_number32_t) var_14 + argument_2 < (uint32_t) &argument_0[31]) {
                argument_0->offset_0 = (pointer_or_number32_t) var_14 + argument_2;
                argument_0->offset_4 = var_18 - argument_2;
              } else {
                argument_0->offset_4 = 0;
                argument_0->offset_0 = &argument_0[1];
              }
              *(generic8_t *) var_14 = (number8_t) argument_2;
              var_3 = &argument_0[2 * ((pointer_or_number32_t) var_14 - (number32_t) argument_0) + 16];
              break;
            }
            var_16 = var_15 - var_18;
            var_17 = var_19;
            if (var_16 < argument_2) {
              var_3 = 0;
              break;
            }
          } else {
            var_17 = (pointer_or_number32_t) var_14 + *(generic8_t *) var_14;
            var_16 = var_15;
          }
          if ((uint32_t) argument_0->offset_0 > var_17) {
            continue;
          }
        }
        var_3 = 0;
        break;
      }
    }
  } else {
    *argument_0->offset_0 = (number8_t) argument_2;
    if ((uint32_t) &argument_0->offset_0[argument_2] < (uint32_t) &argument_0[31]) {
      argument_0->offset_0 = &argument_0->offset_0[argument_2];
      argument_0->offset_4 = (pointer_or_number32_t) argument_0->offset_4 - argument_2;
    } else {
      argument_0->offset_4 = 0;
      argument_0->offset_0 = &argument_0[1];
    }
    var_3 = &argument_0[2 * ((pointer_or_number32_t) argument_0->offset_0 - (number32_t) argument_0) + 16];
  }
  return (struct_536 *) var_3;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405710_Code_x86(void) {
  struct_366 stack;
  generic32_t var_0;
  var_0 = (pointer_or_number32_t) &stack.offset_0.offset_24 + 4;
  if (!segment_2.offset_13928) {
    generic32_t var_1;
    stack.offset_0.offset_24 = "user32.dll";
    var_1 = ((cabifunction_788 *) segment_3.offset_596)();
    if (!var_1) {
      revng_abort("A longjmp was taken");
    }
    generic32_t var_2;
    stack.offset_0.offset_20 = "MessageBoxA";
    stack.offset_0.offset_16 = var_1;
    var_2 = ((cabifunction_789 *) segment_3.offset_600)();
    segment_2.offset_13928 = var_2;
    if (!var_2) {
      revng_abort("A longjmp was taken");
    }
    generic32_t var_3;
    generic32_t var_4;
    stack.offset_0.offset_12 = "GetActiveWindow";
    stack.offset_0.offset_8 = var_1;
    var_4 = ((cabifunction_790 *) segment_3.offset_600)();
    stack.offset_0.offset_4 = "GetLastActivePopup";
    segment_2.offset_13932 = var_4;
    stack.offset_0.offset_0 = var_1;
    var_3 = ((cabifunction_791 *) segment_3.offset_600)();
    segment_2.offset_13936 = var_3;
    var_0 = &stack;
  }
  generic32_t var_5;
  generic32_t var_6;
  var_5 = var_0;
  var_6 = 0;
  if (!segment_2.offset_13932) {
    *(generic8_t **) (var_5 - 4) = ((struct_366 *) var_5)->offset_0.offset_24;
    *(generic8_t **) (var_5 - 8) = ((struct_366 *) var_5)->offset_0.offset_20;
    *(generic32_t *) (var_5 - 12) = ((struct_366 *) var_5)->offset_0.offset_16;
    *(generic32_t *) (var_5 - 16) = var_6;
    ((cabifunction_794 *) segment_2.offset_13928)();
    revng_abort("A longjmp was taken");
  } else {
    generic32_t var_7;
    var_7 = ((cabifunction_792 *) segment_2.offset_13932)();
    var_5 = var_0;
    var_6 = 0;
    if (!var_7) {
      *(generic8_t **) (var_5 - 4) = ((struct_366 *) var_5)->offset_0.offset_24;
      *(generic8_t **) (var_5 - 8) = ((struct_366 *) var_5)->offset_0.offset_20;
      *(generic32_t *) (var_5 - 12) = ((struct_366 *) var_5)->offset_0.offset_16;
      *(generic32_t *) (var_5 - 16) = var_6;
      ((cabifunction_794 *) segment_2.offset_13928)();
      revng_abort("A longjmp was taken");
    } else {
      var_5 = var_0;
      var_6 = var_7;
      if (!segment_2.offset_13936) {
        *(generic8_t **) (var_5 - 4) = ((struct_366 *) var_5)->offset_0.offset_24;
        *(generic8_t **) (var_5 - 8) = ((struct_366 *) var_5)->offset_0.offset_20;
        *(generic32_t *) (var_5 - 12) = ((struct_366 *) var_5)->offset_0.offset_16;
        *(generic32_t *) (var_5 - 16) = var_6;
        ((cabifunction_794 *) segment_2.offset_13928)();
        revng_abort("A longjmp was taken");
      } else {
        generic32_t var_8;
        *(generic32_t *) (var_0 - 4) = var_7;
        var_8 = ((cabifunction_793 *) segment_2.offset_13936)();
        var_6 = var_8;
        var_5 = var_0 - 4;
        *(generic8_t **) (var_5 - 4) = ((struct_366 *) var_5)->offset_0.offset_24;
        *(generic8_t **) (var_5 - 8) = ((struct_366 *) var_5)->offset_0.offset_20;
        *(generic32_t *) (var_5 - 12) = ((struct_366 *) var_5)->offset_0.offset_16;
        *(generic32_t *) (var_5 - 16) = var_6;
        ((cabifunction_794 *) segment_2.offset_13928)();
        revng_abort("A longjmp was taken");
      }
    }
  }
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4057b0_Code_x86(struct_523 *argument_0, struct_527 *argument_1, generic32_t argument_2) {
  uint64_t loop_state_var;
  struct_523 *var_0;
  struct_527 *var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_2;
  if (argument_2) {
    generic32_t var_3;
    generic8_t var_4;
    generic32_t var_5;
    generic32_t var_6;
    generic32_t var_7;
    generic32_t var_8;
    generic32_t var_9;
    generic32_t var_10;
    generic8_t var_11;
    generic32_t var_12;
    generic32_t var_13;
    generic32_t var_14;
    generic32_t var_15;
    generic32_t var_16;
    generic32_t var_17;
    generic32_t var_18;
    generic32_t var_19;
    generic32_t var_20;
    generic32_t var_21;
    generic32_t var_22;
    generic32_t var_23;
    generic32_t var_24;
    generic32_t var_25;
    generic32_t var_26;
    generic32_t var_27;
    generic32_t var_28;
    generic32_t var_29;
    generic32_t var_30;
    generic32_t var_31;
    generic32_t var_32;
    generic32_t var_33;
    generic32_t var_34;
    generic32_t var_35;
    generic32_t var_36;
    generic32_t var_37;
    generic32_t var_38;
    generic32_t var_39;
    generic32_t var_40;
    generic32_t var_41;
    generic32_t var_42;
    generic32_t var_43;
    generic32_t var_44;
    generic32_t var_45;
    generic32_t var_46;
    generic32_t var_47;
    generic32_t var_48;
    generic32_t var_49;
    generic32_t var_50;
    generic32_t var_51;
    generic32_t var_52;
    generic32_t var_53;
    generic32_t var_54;
    generic32_t var_55;
    generic32_t var_56;
    generic32_t var_57;
    generic32_t var_58;
    generic32_t var_59;
    generic32_t var_60;
    generic32_t var_61;
    generic32_t var_62;
    if (!((number32_t) var_1 & 0x3)) {
      var_60 = argument_2 >> 2;
      var_59 = argument_2;
      var_61 = var_0;
      var_62 = var_1;
      if (argument_2 < 4) {
        var_36 = argument_2 >> 1;
        var_34 = argument_2 >> 2;
        var_35 = 40;
        var_38 = argument_2;
        var_39 = var_0;
        var_40 = var_1;
      } else {
        var_56 = var_60;
        var_57 = var_61;
        var_58 = var_62;
        var_13 = var_57 + 4;
        var_12 = var_58 + 4;
        var_55 = 0;
        while (true) {
          var_8 = var_57;
          var_9 = var_12 + (var_55 << 2);
          var_10 = var_13 + (var_55 << 2);
          var_7 = (2164326656 - *(generic32_t *) var_58) ^ *(generic32_t *) var_58;
          if ((var_7 & 0x81010100)) {
            var_54 = 0;
            if (!(*(generic32_t *) var_58 & 0xFF)) {
              var_53 = var_54;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
            var_54 = *(generic32_t *) var_58 & 0xFF;
            if (!(*(generic32_t *) var_58 & 0xFF00)) {
              var_53 = var_54;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
            if (!(*(generic32_t *) var_58 & 0xFF0000)) {
              var_53 = *(generic32_t *) var_58 & 0xFFFF;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
            var_54 = *(generic32_t *) var_58;
            if (*(generic32_t *) var_58 < 16777216) {
              var_53 = var_54;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
          }
          *(generic32_t *) var_8 = *(generic32_t *) var_58;
          var_57 = var_8 + 4;
          var_56 = var_56 - 1;
          var_11 = var_60 == var_55 + 1;
          var_55 = var_55 + 1;
          var_58 = var_58 + 4;
          if (!(var_11)) {
            continue;
          }
          var_41 = var_8 > (uint32_t) -5;
          var_42 = var_7;
          var_43 = var_59;
          var_44 = var_10;
          var_45 = var_9;
          break;
        }
        switch (loop_state_var) {
          case 0:
          case 1:
          {
            if (!(loop_state_var)) {
              var_49 = var_52;
              var_6 = var_49 + (var_51 << 2);
              var_48 = 0;
              do {
                var_48 = var_48 + 1;
                *(generic32_t *) var_49 = 0;
                var_49 = var_49 + 4;
              } while (var_51 != var_48);
              var_46 = var_50;
              var_47 = var_6;
            }
            if (!(var_46 & 0x3)) {
              return *(generic32_t *) (revng_undefined_local_sp() + 4);
            }
            *(generic8_t *) var_47 = '\000';
            var_14 = var_47;
            var_15 = var_46 & 0x3;
            var_16 = 0;
            var_17 = 0;
            while (true) {
              var_24 = var_14 + 1;
              var_19 = var_24;
              var_20 = 28;
              var_18 = var_21;
              switch ((number32_t) var_20) {
                case 28:
                case 30:
                {
                  break;
                } break;
                case 20:
                {
                  var_18 = var_19 > ~var_21;
                } break;
                default:
                {
                  var_18 = 0;
                } break;
              }
              if (var_23 - 1 == 0) {
                break;
              }
              *(generic8_t *) var_24 = (number8_t) var_22;
              var_14 = var_24;
              var_15 = var_23 - 1;
              var_16 = var_22;
              var_17 = var_18;
            }
            return *(generic32_t *) (revng_undefined_local_sp() + 4);
          } break;
        }
        var_36 = var_41;
        var_37 = var_42;
        var_39 = var_44;
        var_40 = var_45;
        var_38 = var_43 & 0x3;
        var_34 = var_38;
        var_35 = 24;
        if (!var_34) {
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
      }
      var_27 = var_34;
      var_28 = var_35;
      var_29 = var_36;
      var_30 = var_37;
      var_31 = var_38;
      var_32 = var_39;
      var_33 = var_40;
      var_5 = var_32 + 1;
      var_26 = 0;
      while (true) {
        var_25 = var_27;
        var_3 = (var_30 & 0xFFFFFF00) | *(generic8_t *) var_33;
        switch ((number32_t) var_28) {
          case 44:
          {
            var_25 = !var_29;
          } break;
          case 43:
          {
            var_25 = !(var_29 & 0xFFFF);
          } break;
          case 42:
          {
            var_25 = !(var_29 & 0xFF);
          } break;
          case 36:
          {
            var_25 = (uint32_t) var_29 >> 31;
          } break;
          case 35:
          {
            var_25 = ((uint32_t) var_29 >> 15) & 0x1;
          } break;
          case 34:
          {
            var_25 = ((uint32_t) var_29 >> 7) & 0x1;
          } break;
          case 20:
          {
            var_25 = var_27 > ~var_29;
          } break;
          case 19:
          {
            var_25 = ((var_27 + var_29) & 0xFFFF) < (var_29 & 0xFFFF);
          } break;
          case 18:
          {
            var_25 = ((var_27 + var_29) & 0xFF) < (var_29 & 0xFF);
          } break;
          case 16:
          {
            var_25 = var_27 > ~var_29;
          } break;
          case 15:
          {
            var_25 = ((var_27 + var_29) & 0xFFFF) < (var_29 & 0xFFFF);
          } break;
          case 14:
          {
            var_25 = ((var_27 + var_29) & 0xFF) < (var_29 & 0xFF);
          } break;
          case 12:
          {
            var_25 = var_27 < var_29;
          } break;
          case 11:
          {
            var_25 = (var_27 & 0xFFFF) < (var_29 & 0xFFFF);
          } break;
          case 10:
          {
            var_25 = (var_27 & 0xFF) < (var_29 & 0xFF);
          } break;
          case 8:
          {
            var_25 = var_27 < var_29;
          } break;
          case 7:
          {
            var_25 = (var_27 & 0xFFFF) < (var_29 & 0xFFFF);
          } break;
          case 6:
          {
            var_25 = (var_27 & 0xFF) < (var_29 & 0xFF);
          } break;
          case 2:
          case 3:
          case 4:
          case 5:
          {
            var_25 = var_29 != 0;
          } break;
          case 26:
          case 27:
          case 28:
          case 29:
          case 30:
          case 31:
          case 32:
          case 33:
          {
            var_25 = var_29;
          } break;
          case 1:
          case 38:
          case 39:
          case 40:
          case 41:
          case 47:
          {
            var_25 = var_29 & 0x1;
          } break;
          case 46:
          case 48:
          {
            break;
          } break;
          default:
          {
            var_25 = 0;
          } break;
        }
        *(generic8_t *) var_32 = *(generic8_t *) var_33;
        if (*(generic8_t *) var_33) {
          var_32 = var_32 + 1;
          var_33 = var_33 + 1;
          var_31 = var_31 - 1;
          var_27 = var_31;
          var_4 = var_38 == var_26 + 1;
          var_26 = var_26 + 1;
          var_28 = 32;
          var_29 = 0;
          var_30 = var_3;
          if (!(var_4)) {
            continue;
          }
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
        var_19 = var_3;
        var_20 = 22;
        var_21 = var_25;
        var_22 = var_3;
        var_23 = var_31;
        var_24 = var_5 + var_26;
        loop_state_var = 0;
        break;
      }
      if (!(loop_state_var)) {
        var_18 = var_21;
        switch ((number32_t) var_20) {
          case 28:
          case 30:
          {
            break;
          } break;
          case 20:
          {
            var_18 = var_19 > ~var_21;
          } break;
          default:
          {
            var_18 = 0;
          } break;
        }
        if (var_23 - 1 == 0) {
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
        *(generic8_t *) var_24 = (number8_t) var_22;
        var_14 = var_24;
        var_15 = var_23 - 1;
        var_16 = var_22;
        var_17 = var_18;
        while (true) {
          var_24 = var_14 + 1;
          var_19 = var_24;
          var_20 = 28;
          var_18 = var_21;
          switch ((number32_t) var_20) {
            case 28:
            case 30:
            {
              break;
            } break;
            case 20:
            {
              var_18 = var_19 > ~var_21;
            } break;
            default:
            {
              var_18 = 0;
            } break;
          }
          if (var_23 - 1 == 0) {
            break;
          }
          *(generic8_t *) var_24 = (number8_t) var_22;
          var_14 = var_24;
          var_15 = var_23 - 1;
          var_16 = var_22;
          var_17 = var_18;
        }
        return *(generic32_t *) (revng_undefined_local_sp() + 4);
      }
    }
    generic32_t var_63;
    generic32_t var_64;
    generic32_t var_65;
    generic32_t var_66;
    var_63 = 0;
    var_64 = argument_2;
    var_65 = var_0;
    var_66 = var_1;
    generic32_t var_67;
    generic32_t var_68;
    generic32_t var_69;
    generic32_t var_70;
    generic32_t var_71;
    generic32_t var_72;
    while (true) {
      generic32_t var_73;
      generic32_t var_74;
      generic32_t var_75;
      generic32_t var_76;
      var_73 = var_64;
      var_74 = (pointer_or_number32_t) var_1 + 1 + var_63;
      var_75 = (pointer_or_number32_t) var_0 + 1 + var_63;
      var_76 = argument_2 - 1 - var_63;
      *(generic8_t *) var_65 = *(generic8_t *) var_66;
      if (argument_2 - 1 == var_63) {
        break;
      }
      if (*(generic8_t *) var_66) {
        var_63 = var_63 + 1;
        var_72 = (var_72 & 0xFFFFFF00) | *(generic8_t *) var_66;
        var_64 = var_73 - 1;
        var_65 = var_65 + 1;
        var_66 = var_66 + 1;
        if ((var_74 & 0x3)) {
          continue;
        }
        var_41 = var_76 >> 1;
        var_42 = var_72;
        var_43 = var_76;
        var_44 = var_75;
        var_45 = var_74;
        if (var_73 < 5) {
          loop_state_var = 1;
          break;
        }
        var_60 = var_76 >> 2;
        var_59 = var_76;
        var_61 = var_75;
        var_62 = var_74;
        loop_state_var = 0;
        break;
      }
      var_67 = var_76;
      var_68 = var_75;
      if (!(var_75 & 0x3)) {
        loop_state_var = 2;
        break;
      }
      var_69 = 0;
      var_70 = var_76;
      var_71 = var_75;
      loop_state_var = 3;
      break;
    }
    switch (loop_state_var) {
      case 0:
      {
        var_56 = var_60;
        var_57 = var_61;
        var_58 = var_62;
        var_13 = var_57 + 4;
        var_12 = var_58 + 4;
        var_55 = 0;
        while (true) {
          var_8 = var_57;
          var_9 = var_12 + (var_55 << 2);
          var_10 = var_13 + (var_55 << 2);
          var_7 = (2164326656 - *(generic32_t *) var_58) ^ *(generic32_t *) var_58;
          if ((var_7 & 0x81010100)) {
            var_54 = 0;
            if (!(*(generic32_t *) var_58 & 0xFF)) {
              var_53 = var_54;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
            var_54 = *(generic32_t *) var_58 & 0xFF;
            if (!(*(generic32_t *) var_58 & 0xFF00)) {
              var_53 = var_54;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
            if (!(*(generic32_t *) var_58 & 0xFF0000)) {
              var_53 = *(generic32_t *) var_58 & 0xFFFF;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
            var_54 = *(generic32_t *) var_58;
            if (*(generic32_t *) var_58 < 16777216) {
              var_53 = var_54;
              *(generic32_t *) var_8 = var_53;
              var_52 = var_8 + 4;
              var_47 = var_52;
              var_51 = var_56 - 1;
              var_46 = var_59;
              var_50 = var_59;
              if (var_56 - 1 == 0) {
                break;
              }
              loop_state_var = 0;
              break;
            }
          }
          *(generic32_t *) var_8 = *(generic32_t *) var_58;
          var_57 = var_8 + 4;
          var_56 = var_56 - 1;
          var_11 = var_60 == var_55 + 1;
          var_55 = var_55 + 1;
          var_58 = var_58 + 4;
          if (!(var_11)) {
            continue;
          }
          var_41 = var_8 > (uint32_t) -5;
          var_42 = var_7;
          var_43 = var_59;
          var_44 = var_10;
          var_45 = var_9;
          break;
        }
        switch (loop_state_var) {
          case 0:
          case 1:
          {
            if (!(loop_state_var)) {
              var_49 = var_52;
              var_6 = var_49 + (var_51 << 2);
              var_48 = 0;
              do {
                var_48 = var_48 + 1;
                *(generic32_t *) var_49 = 0;
                var_49 = var_49 + 4;
              } while (var_51 != var_48);
              var_46 = var_50;
              var_47 = var_6;
            }
            if (!(var_46 & 0x3)) {
              return *(generic32_t *) (revng_undefined_local_sp() + 4);
            }
            *(generic8_t *) var_47 = '\000';
            var_14 = var_47;
            var_15 = var_46 & 0x3;
            var_16 = 0;
            var_17 = 0;
            while (true) {
              var_24 = var_14 + 1;
              var_19 = var_24;
              var_20 = 28;
              var_18 = var_21;
              switch ((number32_t) var_20) {
                case 28:
                case 30:
                {
                  break;
                } break;
                case 20:
                {
                  var_18 = var_19 > ~var_21;
                } break;
                default:
                {
                  var_18 = 0;
                } break;
              }
              if (var_23 - 1 == 0) {
                break;
              }
              *(generic8_t *) var_24 = (number8_t) var_22;
              var_14 = var_24;
              var_15 = var_23 - 1;
              var_16 = var_22;
              var_17 = var_18;
            }
            return *(generic32_t *) (revng_undefined_local_sp() + 4);
          } break;
        }
        var_36 = var_41;
        var_37 = var_42;
        var_39 = var_44;
        var_40 = var_45;
        var_38 = var_43 & 0x3;
        var_34 = var_38;
        var_35 = 24;
        if (!var_34) {
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
        var_27 = var_34;
        var_28 = var_35;
        var_29 = var_36;
        var_30 = var_37;
        var_31 = var_38;
        var_32 = var_39;
        var_33 = var_40;
        var_5 = var_32 + 1;
        var_26 = 0;
        while (true) {
          var_25 = var_27;
          var_3 = (var_30 & 0xFFFFFF00) | *(generic8_t *) var_33;
          switch ((number32_t) var_28) {
            case 44:
            {
              var_25 = !var_29;
            } break;
            case 43:
            {
              var_25 = !(var_29 & 0xFFFF);
            } break;
            case 42:
            {
              var_25 = !(var_29 & 0xFF);
            } break;
            case 36:
            {
              var_25 = (uint32_t) var_29 >> 31;
            } break;
            case 35:
            {
              var_25 = ((uint32_t) var_29 >> 15) & 0x1;
            } break;
            case 34:
            {
              var_25 = ((uint32_t) var_29 >> 7) & 0x1;
            } break;
            case 20:
            {
              var_25 = var_27 > ~var_29;
            } break;
            case 19:
            {
              var_25 = ((var_27 + var_29) & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 18:
            {
              var_25 = ((var_27 + var_29) & 0xFF) < (var_29 & 0xFF);
            } break;
            case 16:
            {
              var_25 = var_27 > ~var_29;
            } break;
            case 15:
            {
              var_25 = ((var_27 + var_29) & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 14:
            {
              var_25 = ((var_27 + var_29) & 0xFF) < (var_29 & 0xFF);
            } break;
            case 12:
            {
              var_25 = var_27 < var_29;
            } break;
            case 11:
            {
              var_25 = (var_27 & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 10:
            {
              var_25 = (var_27 & 0xFF) < (var_29 & 0xFF);
            } break;
            case 8:
            {
              var_25 = var_27 < var_29;
            } break;
            case 7:
            {
              var_25 = (var_27 & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 6:
            {
              var_25 = (var_27 & 0xFF) < (var_29 & 0xFF);
            } break;
            case 2:
            case 3:
            case 4:
            case 5:
            {
              var_25 = var_29 != 0;
            } break;
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
            case 32:
            case 33:
            {
              var_25 = var_29;
            } break;
            case 1:
            case 38:
            case 39:
            case 40:
            case 41:
            case 47:
            {
              var_25 = var_29 & 0x1;
            } break;
            case 46:
            case 48:
            {
              break;
            } break;
            default:
            {
              var_25 = 0;
            } break;
          }
          *(generic8_t *) var_32 = *(generic8_t *) var_33;
          if (*(generic8_t *) var_33) {
            var_32 = var_32 + 1;
            var_33 = var_33 + 1;
            var_31 = var_31 - 1;
            var_27 = var_31;
            var_4 = var_38 == var_26 + 1;
            var_26 = var_26 + 1;
            var_28 = 32;
            var_29 = 0;
            var_30 = var_3;
            if (!(var_4)) {
              continue;
            }
            return *(generic32_t *) (revng_undefined_local_sp() + 4);
          }
          var_19 = var_3;
          var_20 = 22;
          var_21 = var_25;
          var_22 = var_3;
          var_23 = var_31;
          var_24 = var_5 + var_26;
          break;
        }
        var_18 = var_21;
        switch ((number32_t) var_20) {
          case 28:
          case 30:
          {
            break;
          } break;
          case 20:
          {
            var_18 = var_19 > ~var_21;
          } break;
          default:
          {
            var_18 = 0;
          } break;
        }
        if (var_23 - 1 == 0) {
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
        *(generic8_t *) var_24 = (number8_t) var_22;
        var_14 = var_24;
        var_15 = var_23 - 1;
        var_16 = var_22;
        var_17 = var_18;
        while (true) {
          var_24 = var_14 + 1;
          var_19 = var_24;
          var_20 = 28;
          var_18 = var_21;
          switch ((number32_t) var_20) {
            case 28:
            case 30:
            {
              break;
            } break;
            case 20:
            {
              var_18 = var_19 > ~var_21;
            } break;
            default:
            {
              var_18 = 0;
            } break;
          }
          if (var_23 - 1 == 0) {
            break;
          }
          *(generic8_t *) var_24 = (number8_t) var_22;
          var_14 = var_24;
          var_15 = var_23 - 1;
          var_16 = var_22;
          var_17 = var_18;
        }
      } break;
      case 1:
      {
        var_36 = var_41;
        var_37 = var_42;
        var_39 = var_44;
        var_40 = var_45;
        var_38 = var_43 & 0x3;
        var_34 = var_38;
        var_35 = 24;
        if (!var_34) {
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
        var_27 = var_34;
        var_28 = var_35;
        var_29 = var_36;
        var_30 = var_37;
        var_31 = var_38;
        var_32 = var_39;
        var_33 = var_40;
        var_5 = var_32 + 1;
        var_26 = 0;
        while (true) {
          var_25 = var_27;
          var_3 = (var_30 & 0xFFFFFF00) | *(generic8_t *) var_33;
          switch ((number32_t) var_28) {
            case 44:
            {
              var_25 = !var_29;
            } break;
            case 43:
            {
              var_25 = !(var_29 & 0xFFFF);
            } break;
            case 42:
            {
              var_25 = !(var_29 & 0xFF);
            } break;
            case 36:
            {
              var_25 = (uint32_t) var_29 >> 31;
            } break;
            case 35:
            {
              var_25 = ((uint32_t) var_29 >> 15) & 0x1;
            } break;
            case 34:
            {
              var_25 = ((uint32_t) var_29 >> 7) & 0x1;
            } break;
            case 20:
            {
              var_25 = var_27 > ~var_29;
            } break;
            case 19:
            {
              var_25 = ((var_27 + var_29) & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 18:
            {
              var_25 = ((var_27 + var_29) & 0xFF) < (var_29 & 0xFF);
            } break;
            case 16:
            {
              var_25 = var_27 > ~var_29;
            } break;
            case 15:
            {
              var_25 = ((var_27 + var_29) & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 14:
            {
              var_25 = ((var_27 + var_29) & 0xFF) < (var_29 & 0xFF);
            } break;
            case 12:
            {
              var_25 = var_27 < var_29;
            } break;
            case 11:
            {
              var_25 = (var_27 & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 10:
            {
              var_25 = (var_27 & 0xFF) < (var_29 & 0xFF);
            } break;
            case 8:
            {
              var_25 = var_27 < var_29;
            } break;
            case 7:
            {
              var_25 = (var_27 & 0xFFFF) < (var_29 & 0xFFFF);
            } break;
            case 6:
            {
              var_25 = (var_27 & 0xFF) < (var_29 & 0xFF);
            } break;
            case 2:
            case 3:
            case 4:
            case 5:
            {
              var_25 = var_29 != 0;
            } break;
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
            case 32:
            case 33:
            {
              var_25 = var_29;
            } break;
            case 1:
            case 38:
            case 39:
            case 40:
            case 41:
            case 47:
            {
              var_25 = var_29 & 0x1;
            } break;
            case 46:
            case 48:
            {
              break;
            } break;
            default:
            {
              var_25 = 0;
            } break;
          }
          *(generic8_t *) var_32 = *(generic8_t *) var_33;
          if (*(generic8_t *) var_33) {
            var_32 = var_32 + 1;
            var_33 = var_33 + 1;
            var_31 = var_31 - 1;
            var_27 = var_31;
            var_4 = var_38 == var_26 + 1;
            var_26 = var_26 + 1;
            var_28 = 32;
            var_29 = 0;
            var_30 = var_3;
            if (!(var_4)) {
              continue;
            }
            return *(generic32_t *) (revng_undefined_local_sp() + 4);
          }
          var_19 = var_3;
          var_20 = 22;
          var_21 = var_25;
          var_22 = var_3;
          var_23 = var_31;
          var_24 = var_5 + var_26;
          break;
        }
        var_18 = var_21;
        switch ((number32_t) var_20) {
          case 28:
          case 30:
          {
            break;
          } break;
          case 20:
          {
            var_18 = var_19 > ~var_21;
          } break;
          default:
          {
            var_18 = 0;
          } break;
        }
        if (var_23 - 1 == 0) {
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
        *(generic8_t *) var_24 = (number8_t) var_22;
        var_14 = var_24;
        var_15 = var_23 - 1;
        var_16 = var_22;
        var_17 = var_18;
        while (true) {
          var_24 = var_14 + 1;
          var_19 = var_24;
          var_20 = 28;
          var_18 = var_21;
          switch ((number32_t) var_20) {
            case 28:
            case 30:
            {
              break;
            } break;
            case 20:
            {
              var_18 = var_19 > ~var_21;
            } break;
            default:
            {
              var_18 = 0;
            } break;
          }
          if (var_23 - 1 == 0) {
            break;
          }
          *(generic8_t *) var_24 = (number8_t) var_22;
          var_14 = var_24;
          var_15 = var_23 - 1;
          var_16 = var_22;
          var_17 = var_18;
        }
      } break;
      case 2:
      {
        var_50 = var_67;
        var_52 = var_68;
        var_51 = var_50 >> 2;
        if (var_50 < 4) {
          *(generic8_t *) var_68 = '\000';
          var_17 = (var_67 >> 1) & 0x1;
          var_14 = var_68;
          var_15 = var_67;
          var_16 = (var_72 & 0xFFFFFF00) | *(generic8_t *) var_66;
        } else {
          var_49 = var_52;
          var_6 = var_49 + (var_51 << 2);
          var_48 = 0;
          do {
            var_48 = var_48 + 1;
            *(generic32_t *) var_49 = 0;
            var_49 = var_49 + 4;
          } while (var_51 != var_48);
          var_46 = var_50;
          var_47 = var_6;
          if (!(var_46 & 0x3)) {
            return *(generic32_t *) (revng_undefined_local_sp() + 4);
          }
          *(generic8_t *) var_47 = '\000';
          var_14 = var_47;
          var_15 = var_46 & 0x3;
          var_16 = 0;
          var_17 = 0;
        }
        while (true) {
          var_24 = var_14 + 1;
          var_19 = var_24;
          var_20 = 28;
          var_18 = var_21;
          switch ((number32_t) var_20) {
            case 28:
            case 30:
            {
              break;
            } break;
            case 20:
            {
              var_18 = var_19 > ~var_21;
            } break;
            default:
            {
              var_18 = 0;
            } break;
          }
          if (var_23 - 1 == 0) {
            break;
          }
          *(generic8_t *) var_24 = (number8_t) var_22;
          var_14 = var_24;
          var_15 = var_23 - 1;
          var_16 = var_22;
          var_17 = var_18;
        }
      } break;
      case 3:
      {
        while (true) {
          *(generic8_t *) var_71 = '\000';
          if (argument_2 - 2 != var_63 + var_69) {
            generic8_t var_77;
            var_71 = var_71 + 1;
            var_77 = !((var_63 + ((pointer_or_number32_t) var_0 + 2) + var_69) & 0x3);
            var_69 = var_69 + 1;
            var_70 = var_70 - 1;
            if (!(var_77)) {
              continue;
            }
            var_67 = var_70;
            var_68 = var_71;
            break;
          }
          return *(generic32_t *) (revng_undefined_local_sp() + 4);
        }
        var_50 = var_67;
        var_52 = var_68;
        var_51 = var_50 >> 2;
        if (var_50 < 4) {
          *(generic8_t *) var_68 = '\000';
          var_17 = (var_67 >> 1) & 0x1;
          var_14 = var_68;
          var_15 = var_67;
          var_16 = (var_72 & 0xFFFFFF00) | *(generic8_t *) var_66;
        } else {
          var_49 = var_52;
          var_6 = var_49 + (var_51 << 2);
          var_48 = 0;
          do {
            var_48 = var_48 + 1;
            *(generic32_t *) var_49 = 0;
            var_49 = var_49 + 4;
          } while (var_51 != var_48);
          var_46 = var_50;
          var_47 = var_6;
          if (!(var_46 & 0x3)) {
            return *(generic32_t *) (revng_undefined_local_sp() + 4);
          }
          *(generic8_t *) var_47 = '\000';
          var_14 = var_47;
          var_15 = var_46 & 0x3;
          var_16 = 0;
          var_17 = 0;
        }
        while (true) {
          var_24 = var_14 + 1;
          var_19 = var_24;
          var_20 = 28;
          var_18 = var_21;
          switch ((number32_t) var_20) {
            case 28:
            case 30:
            {
              break;
            } break;
            case 20:
            {
              var_18 = var_19 > ~var_21;
            } break;
            default:
            {
              var_18 = 0;
            } break;
          }
          if (var_23 - 1 == 0) {
            break;
          }
          *(generic8_t *) var_24 = (number8_t) var_22;
          var_14 = var_24;
          var_15 = var_23 - 1;
          var_16 = var_22;
          var_17 = var_18;
        }
      } break;
    }
  }
  return *(generic32_t *) (revng_undefined_local_sp() + 4);
}

_ABI(Microsoft_x86_cdecl)
void function_0x4058b0_Code_x86(generic32_t argument_0) {
  uint64_t loop_state_var;
  generic32_t var_0;
  generic32_t var_1;
  generic64_t var_2;
  var_0 = argument_0;
  segment_2.offset_9596 = argument_0;
  var_2 = 4241028;
  if (*(generic32_t *) (generic32_t) 4241024 != argument_0) {
    generic32_t var_3;
    var_3 = 0;
    while (true) {
      generic32_t var_4;
      var_4 = var_3;
      if (!(var_4 > 43)) {
        var_3 = var_4 + 1;
        if (*(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_13960 + var_4 * 8) != argument_0) {
          continue;
        }
        var_2 = (pointer_or_number32_t) &segment_2.offset_13964 + var_4 * 8;
        break;
      }
      var_1 = 13;
      if (argument_0 < 19 || argument_0 > 36) {
        generic32_t var_5;
        var_5 = argument_0 < 188 || argument_0 > 202 ? 22 : 8;
        var_1 = var_5;
        loop_state_var = 1;
      } else {
        loop_state_var = 1;
      }
      break;
    }
    if (loop_state_var == 1) {
      segment_2.offset_9592 = var_1;
      return;
    }
  }
  var_1 = *(generic32_t *) var_2;
  segment_2.offset_9592 = var_1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405920_Code_x86(generic32_t argument_0) {
  struct_377 stack;
  generic32_t var_0;
  var_0 = argument_0;
  if (((segment_2.offset_20320 > argument_0) && ((*(generic8_t *) (*(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + ((argument_0 << 3) & 0xF8) + 4) & 0x1))) && (*(generic32_t *) (*(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + ((argument_0 << 3) & 0xF8)) != (pointer_or_number32_t) -1)) {
    if (segment_2.offset_9696 == 1) {
      generic32_t var_1;
      var_1 = 4294967285;
      switch ((number32_t) argument_0) {
        case 0:
        {
          var_1 = 4294967286;
        } break;
        case 1:
        {
          break;
        } break;
        case 2:
        {
          var_1 = 4294967284;
        } break;
        default:
        {
          *(generic32_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1)) = 4294967295;
          revng_abort("A longjmp was taken");
        } break;
      }
      stack.offset_4 = 0;
      stack.offset_0 = var_1;
      ((cabifunction_795 *) segment_3.offset_592)();
      *(generic32_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1)) = 4294967295;
      revng_abort("A longjmp was taken");
    } else {
      *(generic32_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1)) = 4294967295;
      revng_abort("A longjmp was taken");
    }
  }
  segment_2.offset_9592 = 9;
  segment_2.offset_9596 = 0;
  return 4294967295;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x4059b0_Code_x86(generic32_t argument_0) {
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  if ((segment_2.offset_20320 > argument_0) && ((*(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0x1))) {
    var_1 = *(generic32_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1));
    return var_1;
  }
  segment_2.offset_9592 = 9;
  segment_2.offset_9596 = 0;
  var_1 = 4294967295;
  return var_1;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405a00_Code_x86(generic32_t argument_0, generic32_t argument_1) {
  uint64_t loop_state_var;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  var_0 = argument_0;
  var_1 = argument_1;
  var_2 = argument_0 * argument_1;
  if (!(var_2 > (uint32_t) -32)) {
    var_2 = 16;
    if ((argument_0 * argument_1)) {
      var_2 = (argument_0 * argument_1 + 15) & 0xFFFFFFF0;
    }
  }
  generic32_t var_3;
  var_3 = revng_undefined_local_sp() - 12;
  struct_536 *var_4;
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  while (true) {
    generic32_t var_8;
    var_8 = var_3;
    if (!(var_2 > (uint32_t) -32)) {
      if (!(segment_2.offset_13924 < var_2)) {
        *(generic32_t *) (var_3 - 4) = (uint32_t) var_2 >> 4;
        revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
        var_4 = function_0x405310_Code_x86(0);
        if (var_4) {
          var_5 = var_4;
          if (var_2 < 4) {
            break;
          }
          var_6 = 0;
          var_7 = var_4;
          loop_state_var = 2;
          break;
        }
      }
      generic32_t var_9;
      generic32_t var_10;
      artificial_struct_returned_by_rawfunction_193 var_11;
      *(generic32_t *) (var_3 - 4) = var_2;
      *(generic32_t *) (var_3 - 8) = 8;
      var_8 = var_3 - 12;
      *(generic32_t *) var_8 = segment_2.offset_20052;
      var_11 = ((rawfunction_193 *) segment_3.offset_660)();
      var_10 = var_11.register_eax;
      var_9 = var_11.register_ecx;
      if (var_10) {
        loop_state_var = 0;
        break;
      }
    }
    if (!segment_2.offset_14320) {
      loop_state_var = 0;
      break;
    }
    generic32_t var_12;
    struct_276 var_13;
    *(generic32_t *) (var_8 - 4) = var_2;
    var_12 = function_0x405de0_Code_x86(var_13);
    if (var_12) {
      continue;
    }
    revng_abort("A longjmp was taken");
  }
  if (!(loop_state_var)) {
    revng_abort("A longjmp was taken");
  } else {
    do {
      var_6 = var_6 + 1;
      *(generic32_t *) &((struct_536 *) var_7)->offset_0 = 0;
      var_7 = &((struct_536 *) var_7)->offset_4;
    } while (var_2 >> 2 != var_6);
    var_5 = (var_2 & 0xFFFFFFFC) + (pointer_or_number32_t) var_4;
  }
  if (!(var_2 & 0x3)) {
    revng_abort("A longjmp was taken");
  } else {
    generic32_t var_14;
    generic32_t var_15;
    var_14 = var_2 & 0x3;
    var_15 = var_5;
    do {
      ((struct_536 *) var_15)->offset_0.member_0 = '\000';
      var_14 = var_14 - 1;
      var_15 = &((struct_536 *) var_15)->offset_0.member_1.offset_1;
    } while (var_14 != 0);
    revng_abort("A longjmp was taken");
  }
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405b20_Code_x86(struct_426 *argument_0) {
  struct_378 stack;
  struct_426 *var_0;
  generic32_t var_1;
  var_0 = argument_0;
  if (!argument_0) {
    generic32_t var_2;
    generic32_t var_3;
    *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = 0;
    var_2 = function_0x405bf0_Code_x86(var_3);
    var_1 = var_2;
  } else {
    generic32_t var_4;
    generic32_t var_5;
    *(struct_426 **) ((pointer_or_number32_t) &stack - 4) = argument_0;
    var_4 = function_0x405b70_Code_x86((struct_537 *) var_5);
    var_1 = 4294967295;
    if (!var_4) {
      var_1 = 0;
      if ((argument_0->offset_13 & 0x40)) {
        generic32_t var_6;
        struct_278 var_7;
        *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = argument_0->offset_16;
        var_6 = function_0x405e80_Code_x86(var_7);
        var_1 = var_6 != 0;
      }
    }
  }
  return var_1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405b70_Code_x86(struct_537 *argument_0) {
  struct_380 stack;
  struct_537 *var_0;
  struct_537 *var_1;
  generic32_t var_2;
  var_0 = argument_0;
  if ((((argument_0->offset_12 & 0x3) + 254) & 0xFF) != 0 || !(argument_0->offset_12 & 0x108)) {
    var_1 = argument_0;
    var_2 = 0;
  } else {
    generic32_t var_3;
    generic32_t var_4;
    var_1 = argument_0;
    var_4 = var_1->offset_0 == argument_0->offset_8 ? 64 : 0;
    var_3 = lshift(var_1->offset_0 - argument_0->offset_8, 4294967272);
    var_2 = 0;
    if (!(var_4 | (var_3 & 0x80))) {
      generic32_t var_5;
      generic32_t var_6;
      generic32_t var_7;
      generic32_t var_8;
      generic32_t var_9;
      generic32_t var_10;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = var_1->offset_0 - argument_0->offset_8;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 8) = argument_0->offset_8;
      *(generic32_t *) ((pointer_or_number32_t) &stack - 12) = argument_0->offset_16;
      var_5 = function_0x404790_Code_x86(var_6, (generic8_t *) var_7, var_8);
      if (var_5 == var_1->offset_0 - argument_0->offset_8) {
        var_1 = argument_0;
        var_2 = 0;
        if ((argument_0->offset_12 & 0x80)) {
          var_9 = argument_0->offset_12 & 0xFFFFFFFD;
          var_10 = 0;
          var_2 = var_10;
          argument_0->offset_12 = var_9;
          var_1 = argument_0;
        }
      } else {
        var_9 = argument_0->offset_12 | 0x20;
        var_10 = 4294967295;
        var_2 = var_10;
        argument_0->offset_12 = var_9;
        var_1 = argument_0;
      }
    }
  }
  var_1->offset_0 = argument_0->offset_8;
  argument_0->offset_4 = 0;
  return var_2;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405be0_Code_x86(void) {
  generic32_t var_0;
  generic32_t var_1;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = 1;
  var_0 = function_0x405bf0_Code_x86(var_1);
  return var_0;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405bf0_Code_x86(generic32_t argument_0) {
  struct_379 stack;
  generic32_t var_0;
  generic32_t var_1;
  var_0 = argument_0;
  stack.offset_16 = 0;
  var_1 = 0;
  if ((int32_t) segment_2.offset_20048 > (int32_t) 0) {
    generic32_t var_2;
    generic32_t var_3;
    generic32_t var_4;
    var_2 = 0;
    var_3 = 0;
    var_4 = 0;
    generic8_t var_5;
    generic32_t var_6;
    do {
      var_6 = var_3;
      if (*(generic32_t *) (var_2 + (pointer_or_number32_t) segment_2.offset_15936)) {
        var_6 = var_3;
        if (((number8_t) *(generic32_t *) (*(generic32_t *) (var_2 + (pointer_or_number32_t) segment_2.offset_15936) + 12) & 0x83)) {
          var_6 = var_3;
          switch ((number32_t) var_0) {
            case 1:
            {
              generic32_t var_7;
              generic32_t var_8;
              *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = *(generic32_t *) (var_2 + (pointer_or_number32_t) segment_2.offset_15936);
              var_7 = function_0x405b20_Code_x86((struct_426 *) var_8);
              var_6 = var_3 + (var_7 != (pointer_or_number32_t) -1);
            } break;
            case 0:
            {
              var_6 = var_3;
              if (((number8_t) *(generic32_t *) (*(generic32_t *) (var_2 + (pointer_or_number32_t) segment_2.offset_15936) + 12) & 0x2)) {
                generic32_t var_9;
                generic32_t var_10;
                *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = *(generic32_t *) (var_2 + (pointer_or_number32_t) segment_2.offset_15936);
                var_9 = function_0x405b20_Code_x86((struct_426 *) var_10);
                var_6 = var_3;
                if (var_9 == (pointer_or_number32_t) -1) {
                  stack.offset_16 = 4294967295;
                  var_6 = var_3;
                }
              }
            } break;
          }
        }
      }
      var_5 = (int32_t) (var_4 + 1) < (int32_t) segment_2.offset_20048;
      var_2 = var_2 + 4;
      var_4 = var_4 + 1;
    } while (var_5);
    var_1 = var_6;
  }
  generic32_t var_11;
  var_11 = var_1;
  if (var_0 != 1) {
    var_11 = stack.offset_16;
  }
  return var_11;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405c80_Code_x86(void) {
  generic32_t var_0;
  *(generic32_t *) (revng_undefined_local_sp() - 4) = 2;
  function_0x4029f0_Code_x86(var_0);
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405cd8_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405cec_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405cfc_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405d00_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405d60_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405d70_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405d88_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405d98_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x405d9e_Code_x86(void) {
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405de0_Code_x86(struct_276 argument_0) {
  struct_374 stack;
  generic32_t var_0;
  var_0 = 0;
  if (segment_2.offset_15932) {
    generic32_t var_1;
    *(generic32_t *) &stack = *(generic32_t *) &argument_0;
    var_1 = ((cabifunction_796 *) segment_2.offset_15932)();
    var_0 = var_1 != 0;
  }
  return var_0;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405e10_Code_x86(struct_400 *argument_0) {
  struct_369 stack;
  struct_400 *var_0;
  generic32_t var_1;
  var_0 = argument_0;
  var_1 = 4294967295;
  if (!(((number32_t) argument_0->offset_12 & 0x40) != 0 || !((number32_t) argument_0->offset_12 & 0x83))) {
    generic32_t var_2;
    generic32_t var_3;
    generic32_t var_4;
    generic32_t var_5;
    generic32_t var_6;
    generic32_t var_7;
    *(struct_400 **) ((pointer_or_number32_t) &stack - 4) = argument_0;
    var_4 = function_0x405b70_Code_x86((struct_537 *) var_5);
    *(struct_400 **) ((pointer_or_number32_t) &stack - 4) = argument_0;
    function_0x405fc0_Code_x86((struct_433 *) var_6);
    *(generic32_t *) ((pointer_or_number32_t) &stack - 4) = argument_0->offset_16;
    var_3 = function_0x405ef0_Code_x86(var_7);
    var_2 = lshift(var_3, 4294967272);
    var_1 = 4294967295;
    if (!(var_2 & 0x80)) {
      var_1 = var_4;
      if (argument_0->offset_28) {
        generic32_t var_8;
        *(struct_578 **) ((pointer_or_number32_t) &stack - 4) = argument_0->offset_28;
        function_0x404eb0_Code_x86(var_8);
        argument_0->offset_28 = 0;
        var_1 = var_4;
      }
    }
  }
  argument_0->offset_12 = 0;
  return var_1;
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405e80_Code_x86(struct_278 argument_0) {
  struct_381 stack;
  if ((segment_2.offset_20320 > *(generic32_t *) &argument_0) && ((*(generic8_t *) (((*(generic32_t *) &argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) *(generic32_t *) &argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0x1))) {
    generic32_t var_0;
    generic32_t var_1;
    generic32_t var_2;
    *(generic32_t *) &stack = *(generic32_t *) &argument_0;
    var_1 = function_0x4059b0_Code_x86(var_2);
    *(generic32_t *) &stack = var_1;
    var_0 = ((cabifunction_797 *) segment_3.offset_588)();
    if (!var_0) {
      generic32_t var_3;
      var_3 = ((cabifunction_798 *) segment_3.offset_628)();
      if (var_3) {
        segment_2.offset_9592 = 9;
        segment_2.offset_9596 = var_3;
        return 4294967295;
      }
      revng_abort("A longjmp was taken");
    } else {
      revng_abort("A longjmp was taken");
    }
  }
  segment_2.offset_9592 = 9;
  revng_abort("A longjmp was taken");
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x405ef0_Code_x86(generic32_t argument_0) {
  struct_383 stack;
  generic32_t var_0;
  var_0 = argument_0;
  if ((segment_2.offset_20320 > argument_0) && ((*(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) & 0x1))) {
    generic32_t var_1;
    generic32_t var_2;
    generic32_t var_3;
    generic32_t var_4;
    generic32_t var_5;
    generic32_t var_6;
    if (argument_0 < 3 && argument_0 > 0) {
      generic32_t var_7;
      generic32_t var_8;
      generic32_t var_9;
      generic32_t var_10;
      stack.offset_0 = 2;
      var_8 = function_0x4059b0_Code_x86(var_9);
      stack.offset_0 = 1;
      var_7 = function_0x4059b0_Code_x86(var_10);
      var_5 = (pointer_or_number32_t) &stack + 4;
      var_6 = 0;
      if (var_8 != var_7) {
        stack.offset_0 = argument_0;
        var_3 = function_0x4059b0_Code_x86(var_4);
        stack.offset_0 = var_3;
        var_2 = ((cabifunction_799 *) segment_3.offset_608)();
        var_5 = &stack;
        var_6 = 0;
        if (!var_2) {
          var_1 = ((cabifunction_800 *) segment_3.offset_628)();
          var_6 = var_1;
          var_5 = &stack;
        }
      }
    } else {
      stack.offset_0 = argument_0;
      var_3 = function_0x4059b0_Code_x86(var_4);
      stack.offset_0 = var_3;
      var_2 = ((cabifunction_799 *) segment_3.offset_608)();
      var_5 = &stack;
      var_6 = 0;
      if (!var_2) {
        var_1 = ((cabifunction_800 *) segment_3.offset_628)();
        var_6 = var_1;
        var_5 = &stack;
      }
    }
    generic32_t var_11;
    *(generic32_t *) (var_5 - 4) = argument_0;
    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
    var_11 = function_0x405920_Code_x86(0);
    if (!var_6) {
      *(generic8_t *) (((argument_0 << 3) & 0xF8) + *(generic32_t *) ((pointer_or_number32_t) &segment_2.offset_20064 + (((int32_t) argument_0 >> 3) & 0xFFFFFFFC) * 1) + 4) = '\000';
      revng_abort("A longjmp was taken");
    } else {
      *(generic32_t *) (var_5 - 4) = var_6;
      revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
      function_0x4058b0_Code_x86(0);
      revng_abort("A longjmp was taken");
    }
  }
  segment_2.offset_9592 = 9;
  segment_2.offset_9596 = 0;
  return 4294967295;
}

_ABI(Microsoft_x86_cdecl)
void function_0x405fc0_Code_x86(struct_433 *argument_0) {
  struct_382 stack;
  struct_433 *var_0;
  var_0 = argument_0;
  if (!(!((number8_t) argument_0->offset_12 & 0x83) || !((number8_t) argument_0->offset_12 & 0x8))) {
    generic32_t var_1;
    *(struct_578 **) ((pointer_or_number32_t) &stack - 4) = argument_0->offset_8;
    function_0x404eb0_Code_x86(var_1);
    argument_0->offset_0 = 0;
    argument_0->offset_12 = argument_0->offset_12 & 0xFFFFFBF7;
    argument_0->offset_8 = 0;
    argument_0->offset_4 = 0;
  }
}

_ABI(Microsoft_x86_cdecl)
void function_0x406000_Code_x86(void) {
}

_ABI(Microsoft_x86_cdecl)
struct_436 *function_0x406010_Code_x86(struct_436 *argument_0) {
  struct_384 stack;
  struct_436 *var_0;
  var_0 = argument_0;
  stack.offset_28 = 0;
  if (segment_2.offset_11824) {
    stack.offset_24 = 0;
    stack.offset_20 = 0;
    stack.offset_16 = 0;
    stack.offset_12 = 4294967295;
    stack.offset_8 = var_0;
    stack.offset_4 = 512;
    stack.offset_0 = segment_2.offset_11824;
    function_0x4060d0_Code_x86();
  }
  if (*(generic8_t *) var_0) {
    generic32_t var_1;
    generic8_t var_2;
    generic32_t var_3;
    var_1 = 0;
    var_2 = *(generic8_t *) var_0;
    var_3 = var_0;
    generic8_t var_4;
    generic32_t var_5;
    do {
      var_4 = var_2;
      if (!(var_4 < 'a' || var_4 > 'z')) {
        var_4 = var_2 - ' ';
        *(generic8_t *) var_3 = var_4;
      }
      var_5 = (var_5 & 0xFFFFFF00) | var_4;
      var_3 = var_3 + 1;
      var_2 = *(generic8_t *) ((pointer_or_number32_t) var_0 + 1 + var_1);
      var_1 = var_1 + 1;
    } while (var_2);
  }
  return var_0;
}

_ABI(Microsoft_x86_cdecl) _Noreturn
void function_0x4060d0_Code_x86(void) {
  struct_385 stack;
  generic32_t var_0;
  generic32_t var_1;
  generic32_t var_2;
  if (!segment_2.offset_14328) {
    generic32_t var_3;
    stack.offset_44 = 0;
    var_2 = segment_3.offset_604;
    stack.offset_40 = 0;
    stack.offset_36 = 1;
    stack.offset_32 = (pointer_or_number32_t) &segment_2 + 9468;
    stack.offset_28 = 256;
    stack.offset_24 = 0;
    var_3 = ((cabifunction_802 *) var_2)();
    var_0 = &stack.offset_24;
    var_1 = 2;
    if (!var_3) {
      generic32_t var_4;
      stack.offset_20 = 0;
      stack.offset_16 = 0;
      stack.offset_12 = 1;
      stack.offset_8 = "";
      stack.offset_4 = 256;
      stack.offset_0 = 0;
      var_4 = ((cabifunction_803 *) segment_3.offset_584)();
      var_0 = &stack;
      var_1 = 1;
      var_2 = segment_3.offset_604;
      if (!var_4) {
        revng_abort("A longjmp was taken");
      }
    }
  } else {
    var_2 = segment_3.offset_604;
    var_0 = &stack.offset_48;
    var_1 = segment_2.offset_14328;
  }
  generic32_t var_5;
  generic32_t var_6;
  generic32_t var_7;
  generic32_t var_8;
  var_7 = var_1;
  var_8 = ((struct_385 *) var_0)->offset_32;
  segment_2.offset_14328 = var_7;
  var_6 = !var_8 ? 64 : 0;
  var_5 = lshift(var_8, 4294967272);
  if (!(var_6 | (var_5 & 0x80))) {
    generic32_t var_9;
    *(generic32_t *) (var_0 - 4) = ((struct_385 *) var_0)->offset_32;
    *(generic32_t *) (var_0 - 8) = ((struct_385 *) var_0)->offset_28;
    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
    var_9 = function_0x406300_Code_x86((struct_540 *) NULL, 0);
    var_8 = var_9;
    var_7 = segment_2.offset_14328;
  }
  segment_2.offset_14328 = var_7;
  switch ((number32_t) var_7) {
    case 2:
    {
      *(struct_578 **) (var_0 - 4) = ((struct_385 *) var_0)->offset_40;
      *(generic32_t *) (var_0 - 8) = ((struct_385 *) var_0)->offset_36;
      *(generic32_t *) (var_0 - 12) = var_8;
      *(generic32_t *) (var_0 - 16) = ((struct_385 *) var_0)->offset_28;
      *(struct_578 **) (var_0 - 20) = ((struct_385 *) var_0)->offset_24;
      *(struct_578 **) (var_0 - 24) = ((struct_385 *) var_0)->offset_20;
      ((cabifunction_804 *) var_2)();
      revng_abort("A longjmp was taken");
    } break;
    case 1:
    {
      break;
    } break;
    default:
    {
      revng_abort("A longjmp was taken");
    } break;
  }
  if (!((struct_385 *) var_0)->offset_44) {
    ((struct_385 *) var_0)->offset_44 = segment_2.offset_11840;
  }
  generic32_t var_10;
  *(generic32_t *) (var_0 - 4) = 0;
  *(generic32_t *) (var_0 - 8) = 0;
  *(generic32_t *) (var_0 - 12) = var_8;
  *(generic32_t *) (var_0 - 16) = ((struct_385 *) var_0)->offset_28;
  *(generic32_t *) (var_0 - 20) = 9;
  *(struct_578 **) (var_0 - 24) = ((struct_385 *) var_0)->offset_44;
  var_10 = ((cabifunction_805 *) segment_3.offset_568)();
  if (!var_10) {
    revng_abort("A longjmp was taken");
  } else {
    union_418 *var_11;
    *(generic32_t *) (var_0 - 28) = var_10 << 1;
    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
    var_11 = function_0x404f00_Code_x86(0);
    if (!var_11) {
      revng_abort("A longjmp was taken");
    } else {
      generic32_t var_12;
      generic32_t var_13;
      generic32_t var_14;
      generic32_t var_15;
      generic32_t var_16;
      artificial_struct_returned_by_rawfunction_205 var_17;
      *(generic32_t *) (var_0 - 28) = var_10;
      *(union_418 **) (var_0 - 32) = var_11;
      *(generic32_t *) (var_0 - 36) = var_8;
      *(generic32_t *) (var_0 - 40) = ((struct_385 *) var_0)->offset_4;
      *(generic32_t *) (var_0 - 44) = 1;
      var_15 = var_0 - 48;
      var_12 = var_15;
      *(struct_578 **) var_12 = ((struct_385 *) var_0)->offset_20;
      var_17 = ((rawfunction_205 *) segment_3.offset_568)();
      var_14 = var_17.register_eax;
      var_13 = var_17.register_ecx;
      var_16 = 0;
      if (var_14) {
        generic32_t var_18;
        generic32_t var_19;
        artificial_struct_returned_by_rawfunction_206 var_20;
        *(generic32_t *) (var_0 - 52) = 0;
        *(generic32_t *) (var_0 - 56) = 0;
        *(generic32_t *) (var_0 - 60) = var_10;
        *(union_418 **) (var_0 - 64) = var_11;
        *(generic32_t *) (var_0 - 68) = *(generic32_t *) (var_0 - 24);
        var_15 = var_0 - 72;
        *(generic32_t *) var_15 = *(generic32_t *) (var_0 - 28);
        var_20 = ((rawfunction_206 *) segment_3.offset_584)();
        var_19 = var_20.register_eax;
        var_18 = var_20.register_ecx;
        var_16 = 0;
        if (var_19) {
          generic32_t var_21;
          generic32_t var_22;
          if (!(*(generic8_t *) (var_0 - 47) & 0x4)) {
            union_418 *var_23;
            *(generic32_t *) (var_0 - 76) = var_19 << 1;
            revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
            var_23 = function_0x404f00_Code_x86(0);
            var_15 = var_0 - 72;
            var_16 = 0;
            if (var_23) {
              generic32_t var_24;
              generic32_t var_25;
              artificial_struct_returned_by_rawfunction_208 var_26;
              *(generic32_t *) (var_0 - 76) = var_19;
              *(union_418 **) (var_0 - 80) = var_23;
              *(generic32_t *) (var_0 - 84) = var_10;
              *(union_418 **) (var_0 - 88) = var_11;
              *(generic32_t *) (var_0 - 92) = *(generic32_t *) var_12;
              var_15 = var_0 - 96;
              *(generic32_t *) var_15 = *(generic32_t *) (var_0 - 52);
              var_26 = ((rawfunction_208 *) segment_3.offset_584)();
              var_25 = var_26.register_eax;
              var_24 = var_26.register_ecx;
              var_16 = var_23;
              if (var_25) {
                *(generic32_t *) (var_0 - 100) = 0;
                *(generic32_t *) (var_0 - 104) = 0;
                if (!*(generic32_t *) (var_0 - 56)) {
                  generic32_t var_27;
                  generic32_t var_28;
                  artificial_struct_returned_by_rawfunction_209 var_29;
                  *(generic32_t *) (var_0 - 108) = 0;
                  *(generic32_t *) (var_0 - 112) = 0;
                  *(generic32_t *) (var_0 - 116) = var_19;
                  *(union_418 **) (var_0 - 120) = var_23;
                  *(generic32_t *) (var_0 - 124) = 544;
                  var_21 = var_0 - 128;
                  var_15 = var_21;
                  *(generic32_t *) var_15 = *(generic32_t *) (var_0 - 52);
                  var_29 = ((rawfunction_209 *) segment_3.offset_724)();
                  var_28 = var_29.register_eax;
                  var_27 = var_29.register_ecx;
                  var_16 = var_23;
                  var_22 = var_23;
                  if (var_28) {
                    *(union_418 **) (var_21 - 4) = var_11;
                    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
                    function_0x404eb0_Code_x86(0);
                    *(generic32_t *) (var_21 - 4) = var_22;
                    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
                    function_0x404eb0_Code_x86(0);
                    revng_abort("A longjmp was taken");
                  }
                } else {
                  generic32_t var_30;
                  generic32_t var_31;
                  artificial_struct_returned_by_rawfunction_210 var_32;
                  *(generic32_t *) (var_0 - 108) = *(generic32_t *) (var_0 - 56);
                  *(generic32_t *) (var_0 - 112) = *(generic32_t *) (var_0 - 60);
                  *(generic32_t *) (var_0 - 116) = var_19;
                  *(union_418 **) (var_0 - 120) = var_23;
                  *(generic32_t *) (var_0 - 124) = 544;
                  var_21 = var_0 - 128;
                  var_15 = var_21;
                  *(generic32_t *) var_15 = *(generic32_t *) (var_0 - 52);
                  var_32 = ((rawfunction_210 *) segment_3.offset_724)();
                  var_31 = var_32.register_eax;
                  var_30 = var_32.register_ecx;
                  var_16 = var_23;
                  var_22 = var_23;
                  if (var_31) {
                    *(union_418 **) (var_21 - 4) = var_11;
                    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
                    function_0x404eb0_Code_x86(0);
                    *(generic32_t *) (var_21 - 4) = var_22;
                    revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
                    function_0x404eb0_Code_x86(0);
                    revng_abort("A longjmp was taken");
                  }
                }
              }
            }
          } else {
            var_21 = var_0 - 72;
            var_22 = 0;
            if (!*(generic32_t *) (var_0 - 32)) {
              *(union_418 **) (var_21 - 4) = var_11;
              revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
              function_0x404eb0_Code_x86(0);
              *(generic32_t *) (var_21 - 4) = var_22;
              revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
              function_0x404eb0_Code_x86(0);
              revng_abort("A longjmp was taken");
            }
            var_15 = var_0 - 72;
            var_16 = 0;
            if (!((int32_t) var_19 > (int32_t) *(generic32_t *) (var_0 - 32))) {
              generic32_t var_33;
              generic32_t var_34;
              artificial_struct_returned_by_rawfunction_207 var_35;
              *(generic32_t *) (var_0 - 76) = *(generic32_t *) (var_0 - 32);
              *(generic32_t *) (var_0 - 80) = *(generic32_t *) (var_0 - 36);
              *(generic32_t *) (var_0 - 84) = var_10;
              *(union_418 **) (var_0 - 88) = var_11;
              *(generic32_t *) (var_0 - 92) = *(generic32_t *) var_12;
              var_21 = var_0 - 96;
              var_15 = var_21;
              *(generic32_t *) var_15 = *(generic32_t *) (var_0 - 52);
              var_35 = ((rawfunction_207 *) segment_3.offset_584)();
              var_34 = var_35.register_eax;
              var_33 = var_35.register_ecx;
              var_16 = 0;
              var_22 = 0;
              if (var_34) {
                *(union_418 **) (var_21 - 4) = var_11;
                revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
                function_0x404eb0_Code_x86(0);
                *(generic32_t *) (var_21 - 4) = var_22;
                revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
                function_0x404eb0_Code_x86(0);
                revng_abort("A longjmp was taken");
              }
            }
          }
        }
      }
      *(union_418 **) (var_15 - 4) = var_11;
      revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
      function_0x404eb0_Code_x86(0);
      *(generic32_t *) (var_15 - 4) = var_16;
      revng_abort("Ignoring stack arguments for this call site: stack size at call site unknown");
      function_0x404eb0_Code_x86(0);
      revng_abort("A longjmp was taken");
    }
  }
}

_ABI(Microsoft_x86_cdecl)
generic32_t function_0x406300_Code_x86(struct_540 *argument_0, generic32_t argument_1) {
  struct_540 *var_0;
  generic32_t var_1;
  generic32_t var_2;
  generic32_t var_3;
  generic32_t var_4;
  var_0 = argument_0;
  var_1 = argument_1;
  var_4 = argument_0;
  if (argument_1) {
    generic32_t var_5;
    generic32_t var_6;
    var_5 = 0;
    var_6 = argument_0;
    while (true) {
      generic32_t var_7;
      var_7 = var_5;
      if (((struct_540 *) var_6)->offset_0) {
        var_5 = var_7 + 1;
        var_6 = &((struct_540 *) var_6)->offset_1;
        if (~var_7 != 0 - argument_1) {
          continue;
        }
        var_4 = (pointer_or_number32_t) &argument_0->offset_1 + var_7 * 1;
        break;
      }
      var_3 = var_6;
      var_2 = var_3 - (number32_t) argument_0;
      return var_2;
    }
  }
  var_3 = var_4;
  var_2 = argument_1;
  if (((struct_540 *) var_3)->offset_0) {
    return var_2;
  }
  var_2 = var_3 - (number32_t) argument_0;
  return var_2;
}

