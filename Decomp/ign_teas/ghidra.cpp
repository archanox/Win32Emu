#include "out.h"



undefined4 __cdecl FUN_00401000(undefined2 *param_1,int param_2)

{
  int iVar1;
  int *piVar2;
  undefined2 *puVar3;
  undefined2 *puVar4;
  
  puVar3 = &DAT_00452e30;
  iVar1 = 0;
  piVar2 = &DAT_00452a20;
  puVar4 = param_1;
  do {
    *puVar3 = *puVar4;
    puVar4 = puVar4 + 1;
    puVar3 = puVar3 + 1;
    *piVar2 = (int)*(char *)((int)param_1 + iVar1 + 0x200) << 4;
    iVar1 = iVar1 + 1;
    piVar2 = piVar2 + 1;
  } while (puVar3 < &DAT_00453030);
  DAT_00452e20 = 0x80;
  DAT_00453034 = param_1 + 0x180;
  DAT_00453030 = 0;
  DAT_00453038 = param_2 * 2 + -0x600;
  DAT_0045303c = 0;
  return 1;
}



longlong __fastcall FUN_00401080(undefined4 param_1,uint param_2,short *param_3,uint param_4)

{
  short sVar1;
  uint uVar2;
  uint uVar3;
  int iVar4;
  byte *pbVar5;
  
  if (DAT_0045303c == DAT_00453038) {
    return (ulonglong)param_2 << 0x20;
  }
  uVar3 = param_4;
  if ((int)DAT_00453038 < (int)param_4) {
    uVar3 = DAT_00453038;
  }
  pbVar5 = (byte *)(DAT_00453034 + (DAT_0045303c >> 1));
  uVar2 = uVar3;
  if ((DAT_0045303c & 1) != 0) {
    iVar4 = DAT_00452e20 + (uint)(*pbVar5 >> 4);
    DAT_00453030 = DAT_00453030 + (&DAT_00452e30)[iVar4];
    DAT_00452e20 = (&DAT_00452a20)[iVar4];
    *param_3 = DAT_00453030;
    param_3 = param_3 + 1;
    pbVar5 = pbVar5 + 1;
    uVar2 = uVar3 - 1;
    if (uVar3 - 1 == 0) goto LAB_00401151;
  }
  do {
    iVar4 = DAT_00452e20 + (*pbVar5 & 0xffffff0f);
    sVar1 = (&DAT_00452e30)[iVar4];
    iVar4 = (&DAT_00452a20)[iVar4];
    *param_3 = DAT_00453030 + sVar1;
    iVar4 = iVar4 + (uint)(*pbVar5 >> 4);
    DAT_00453030 = DAT_00453030 + sVar1 + (&DAT_00452e30)[iVar4];
    DAT_00452e20 = (&DAT_00452a20)[iVar4];
    param_3[1] = DAT_00453030;
    param_3 = param_3 + 2;
    pbVar5 = pbVar5 + 1;
    uVar3 = uVar2 - 2;
    iVar4 = uVar2 - 3;
    uVar2 = uVar3;
  } while (1 < (int)uVar3);
  if (SBORROW4(uVar3,1) == iVar4 < 0) {
    iVar4 = DAT_00452e20 + (*pbVar5 & 0xffffff0f);
    DAT_00453030 = DAT_00453030 + (&DAT_00452e30)[iVar4];
    DAT_00452e20 = (&DAT_00452a20)[iVar4];
    *param_3 = DAT_00453030;
  }
LAB_00401151:
  if ((int)DAT_00453038 < (int)(DAT_0045303c + param_4)) {
    iVar4 = DAT_00453038 - DAT_0045303c;
    DAT_0045303c = DAT_00453038;
    return CONCAT44(param_2,iVar4);
  }
  DAT_0045303c = DAT_0045303c + param_4;
  return CONCAT44(param_2,param_4);
}



undefined4 FUN_004011a0(void)

{
  long lVar1;
  void *pvVar2;
  char *pcVar3;
  int iVar4;
  
  iVar4 = 0;
  pcVar3 = s_data_IGN1_DPS_0041c038;
  do {
    lVar1 = FUN_004044d0(pcVar3);
    *(long *)((int)&DAT_0043c3a0 + iVar4) = lVar1;
    if (lVar1 < 0x300) {
      return 0;
    }
    pvVar2 = FUN_004043a0(pcVar3);
    *(void **)((int)&DAT_0043c3d8 + iVar4) = pvVar2;
    if (pvVar2 == (void *)0x0) {
      return 0;
    }
    iVar4 = iVar4 + 4;
    pcVar3 = pcVar3 + 0x32;
  } while (pcVar3 < &DAT_0041c1fa);
  FUN_00401000((undefined2 *)(&DAT_0043c3d8)[*(int *)(&DAT_0041c200 + DAT_0041c520 * 4)],
               (&DAT_0043c3a0)[*(int *)(&DAT_0041c200 + DAT_0041c520 * 4)]);
  DAT_00453098 = 0x14;
  iVar4 = FUN_00403d20();
  if (iVar4 != 1) {
    return 0;
  }
  DAT_0043c3c8 = DAT_00453088;
  DAT_0043c400 = DAT_00453084;
  DAT_0043c3d0 = DAT_00453090;
  DAT_0043c3fc = DAT_0045308c;
  if (43999 < DAT_00453080) {
    DAT_0043c3fc = DAT_0045308c / 2;
  }
  DAT_0043c3c4 = DAT_00453080;
  DAT_0043c3cc = FUN_00403630(0,DAT_0043c3fc * 2);
  if (DAT_0043c3cc == (void *)0x0) {
    return 0;
  }
  DAT_0041c030 = 1;
  return 1;
}



undefined4 FUN_004012a0(void)

{
  uint uVar1;
  undefined4 *puVar2;
  int iVar3;
  undefined4 extraout_ECX;
  uint extraout_EDX;
  uint extraout_EDX_00;
  uint extraout_EDX_01;
  longlong lVar4;
  
  if (DAT_0041c030 == 1) {
    puVar2 = (undefined4 *)FUN_00403910();
    uVar1 = extraout_EDX;
    while (puVar2 != (undefined4 *)0x0) {
      lVar4 = FUN_00401080(DAT_0043c3cc,uVar1,(short *)DAT_0043c3cc,DAT_0043c3fc);
      iVar3 = (int)lVar4;
      if (iVar3 < (int)DAT_0043c3fc) {
        DAT_0041c520 = DAT_0041c520 + 1;
        if (*(int *)(&DAT_0041c200 + DAT_0041c520 * 4) == -1) {
          DAT_0041c520 = 9;
        }
        FUN_00401000((undefined2 *)(&DAT_0043c3d8)[*(int *)(&DAT_0041c200 + DAT_0041c520 * 4)],
                     (&DAT_0043c3a0)[*(int *)(&DAT_0041c200 + DAT_0041c520 * 4)]);
        FUN_00401080(extraout_ECX,extraout_EDX_00,(short *)(iVar3 * 2 + DAT_0043c3cc),
                     DAT_0043c3fc - iVar3);
      }
      FUN_004013c0((ushort *)*puVar2,DAT_0043c3cc,puVar2[2],0);
      if (((ushort *)puVar2[1] != (ushort *)0x0) && (puVar2[3] != 0)) {
        FUN_004013c0((ushort *)puVar2[1],DAT_0043c3cc,puVar2[3],puVar2[2]);
      }
      FUN_00403bf0();
      puVar2 = (undefined4 *)FUN_00403910();
      uVar1 = extraout_EDX_01;
    }
  }
  return 1;
}



undefined4 FUN_004013a0(void)

{
  DAT_0041c030 = 1;
  FUN_00403cb0();
  DAT_0041c030 = 0;
  return 1;
}



undefined4 __cdecl FUN_004013c0(ushort *param_1,int param_2,uint param_3,int param_4)

{
  undefined1 uVar1;
  uint uVar2;
  ushort *puVar3;
  ushort *puVar4;
  byte bVar5;
  ushort uVar6;
  uint uVar7;
  ushort *puVar8;
  ushort *local_4;
  
  puVar8 = local_4;
  if (43999 < DAT_0043c3c4) {
    uVar7 = (int)param_3 >> 0x1f;
    uVar2 = param_3 ^ uVar7;
    param_3 = (int)param_3 / 2;
    param_4 = param_4 / 2;
    puVar8 = (ushort *)((uVar2 - uVar7 & 1 ^ uVar7) - uVar7);
  }
  puVar3 = (ushort *)(param_2 + param_4 * 2);
  if (DAT_0043c400 == 0x10) {
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 0)) && (DAT_0043c3c4 < 44000)) {
      local_4 = param_1;
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          uVar6 = *puVar3;
          local_4 = puVar4 + 2;
          *puVar4 = uVar6 ^ 0x8000;
          puVar3 = puVar3 + 1;
          puVar4[1] = uVar6 ^ 0x8000;
          param_3 = param_3 - 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 0)) && (43999 < DAT_0043c3c4)) {
      puVar4 = param_1;
      local_4 = param_1;
      if (0 < (int)param_3) {
        do {
          uVar6 = *puVar3 ^ 0x8000;
          *puVar4 = uVar6;
          puVar4[1] = uVar6;
          local_4 = puVar4 + 4;
          puVar4[2] = uVar6;
          puVar3 = puVar3 + 1;
          puVar4[3] = uVar6;
          param_3 = param_3 - 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
  }
  if (DAT_0043c400 == 0x10) {
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 1)) && (DAT_0043c3c4 < 44000)) {
      local_4 = param_1;
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          uVar6 = *puVar3;
          *puVar4 = uVar6;
          local_4 = puVar4 + 2;
          puVar4[1] = uVar6;
          puVar3 = puVar3 + 1;
          param_3 = param_3 - 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 1)) && (43999 < DAT_0043c3c4)) {
      local_4 = param_1;
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          uVar6 = *puVar3;
          *puVar4 = uVar6;
          puVar4[1] = uVar6;
          puVar4[2] = uVar6;
          local_4 = puVar4 + 4;
          puVar4[3] = uVar6;
          puVar3 = puVar3 + 1;
          param_3 = param_3 - 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
  }
  if (DAT_0043c400 == 0x10) {
    if (((DAT_0043c3c8 == 0) && (DAT_0043c3d0 == 0)) && (DAT_0043c3c4 < 44000)) {
      local_4 = param_1;
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          uVar6 = *puVar3;
          local_4 = puVar4 + 1;
          puVar3 = puVar3 + 1;
          *puVar4 = uVar6 ^ 0x8000;
          param_3 = param_3 - 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
    if (((DAT_0043c3c8 == 0) && (DAT_0043c3d0 == 0)) && (43999 < DAT_0043c3c4)) {
      local_4 = param_1;
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          local_4 = puVar4 + 2;
          *puVar4 = *puVar3 ^ 0x8000;
          param_3 = param_3 - 1;
          puVar4[1] = *puVar3 ^ 0x8000;
          puVar3 = puVar3 + 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
  }
  if (DAT_0043c400 == 0x10) {
    if (((DAT_0043c3c8 == 0) && (DAT_0043c3d0 == 1)) && (DAT_0043c3c4 < 44000)) {
      local_4 = param_1;
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          local_4 = puVar4 + 1;
          *puVar4 = *puVar3;
          puVar3 = puVar3 + 1;
          param_3 = param_3 - 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
    if (((DAT_0043c3c8 == 0) && (DAT_0043c3d0 == 1)) && (43999 < DAT_0043c3c4)) {
      local_4 = param_1;
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          *puVar4 = *puVar3;
          local_4 = puVar4 + 2;
          uVar6 = *puVar3;
          puVar3 = puVar3 + 1;
          puVar4[1] = uVar6;
          param_3 = param_3 - 1;
          puVar4 = local_4;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
  }
  if (DAT_0043c400 == 8) {
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 0)) && (DAT_0043c3c4 < 44000)) {
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          param_1 = puVar4 + 1;
          *(byte *)puVar4 = (byte)(*puVar3 >> 8) ^ 0x80;
          param_3 = param_3 - 1;
          *(byte *)((int)puVar4 + 1) = (byte)(*puVar3 >> 8) ^ 0x80;
          puVar3 = puVar3 + 1;
          puVar4 = param_1;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 0)) && (43999 < DAT_0043c3c4)) {
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          bVar5 = (byte)(*puVar3 >> 8) ^ 0x80;
          param_1 = puVar4 + 2;
          puVar3 = puVar3 + 1;
          param_3 = param_3 - 1;
          *(byte *)puVar4 = bVar5;
          *(byte *)((int)puVar4 + 1) = bVar5;
          *(byte *)(puVar4 + 1) = bVar5;
          *(byte *)((int)puVar4 + 3) = bVar5;
          puVar4 = param_1;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
  }
  if (DAT_0043c400 == 8) {
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 1)) && (DAT_0043c3c4 < 44000)) {
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          param_1 = puVar4 + 1;
          param_3 = param_3 - 1;
          *(char *)puVar4 = (char)(*puVar3 >> 8);
          *(char *)((int)puVar4 + 1) = (char)(*puVar3 >> 8);
          puVar3 = puVar3 + 1;
          puVar4 = param_1;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
    if (((DAT_0043c3c8 == 1) && (DAT_0043c3d0 == 1)) && (43999 < DAT_0043c3c4)) {
      if (0 < (int)param_3) {
        do {
          uVar1 = *(undefined1 *)((int)puVar3 + 1);
          puVar3 = puVar3 + 1;
          *(undefined1 *)param_1 = uVar1;
          *(undefined1 *)((int)param_1 + 1) = uVar1;
          *(undefined1 *)(param_1 + 1) = uVar1;
          *(undefined1 *)((int)param_1 + 3) = uVar1;
          param_1 = param_1 + 2;
          param_3 = param_3 - 1;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
  }
  if (DAT_0043c400 == 8) {
    if ((DAT_0043c3c8 == 0) && ((DAT_0043c3d0 == 0) < 44000)) {
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          uVar6 = *puVar3;
          param_1 = (ushort *)((int)puVar4 + 1);
          puVar3 = puVar3 + 1;
          param_3 = param_3 - 1;
          *(byte *)puVar4 = (byte)(uVar6 >> 8) ^ 0x80;
          puVar4 = param_1;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
    if ((DAT_0043c3c8 == 0) && (43999 < (DAT_0043c3d0 == 0))) {
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          param_1 = puVar4 + 1;
          *(byte *)puVar4 = (byte)(*puVar3 >> 8) ^ 0x80;
          param_3 = param_3 - 1;
          *(byte *)((int)puVar4 + 1) = (byte)(*puVar3 >> 8) ^ 0x80;
          puVar3 = puVar3 + 1;
          puVar4 = param_1;
        } while (param_3 != 0);
      }
      goto LAB_00401976;
    }
  }
  if (DAT_0043c400 == 8) {
    if ((DAT_0043c3c8 == 0) && ((DAT_0043c3d0 == 1) < 44000)) {
      puVar4 = param_1;
      if (0 < (int)param_3) {
        do {
          uVar6 = *puVar3;
          param_1 = (ushort *)((int)puVar4 + 1);
          puVar3 = puVar3 + 1;
          param_3 = param_3 - 1;
          *(char *)puVar4 = (char)(uVar6 >> 8);
          puVar4 = param_1;
        } while (param_3 != 0);
      }
    }
    else if ((DAT_0043c3c8 == 0) &&
            ((43999 < (DAT_0043c3d0 == 1) && (puVar4 = param_1, 0 < (int)param_3)))) {
      do {
        param_1 = puVar4 + 1;
        param_3 = param_3 - 1;
        *(char *)puVar4 = (char)(*puVar3 >> 8);
        *(char *)((int)puVar4 + 1) = (char)(*puVar3 >> 8);
        puVar3 = puVar3 + 1;
        puVar4 = param_1;
      } while (param_3 != 0);
    }
  }
LAB_00401976:
  if ((43999 < DAT_0043c3c4) && (puVar8 == (ushort *)0x1)) {
    if (DAT_0043c400 == 0x10) {
      param_1 = local_4;
    }
    if (DAT_0043c400 == 8) {
      *(undefined1 *)param_1 = *(undefined1 *)((int)param_1 + -1);
    }
    else if ((DAT_0043c400 == 0x10) && (DAT_0043c3c8 == 0)) {
      *param_1 = param_1[-1];
    }
    else {
      *(undefined4 *)param_1 = *(undefined4 *)(param_1 + -2);
    }
  }
  return 1;
}



int __cdecl FUN_004019d0(int *param_1,int param_2,int *param_3)

{
  int iVar1;
  int *piVar2;
  int iVar3;
  
  iVar3 = 0;
  DAT_00452a10 = FUN_004035a0(s_Script_Player_0041c524);
  if (DAT_00452a10 == -1) {
    return -1;
  }
  if (*param_3 != 0) {
    piVar2 = &DAT_0043c420;
    do {
      if ((int *)0x43c45f < piVar2) break;
      iVar1 = *param_3;
      param_3 = param_3 + 1;
      *piVar2 = iVar1;
      piVar2 = piVar2 + 1;
    } while (*param_3 != 0);
  }
  iVar1 = *param_1;
  piVar2 = param_1;
  while (iVar1 != 0x7878696c) {
    iVar1 = *piVar2;
    if (iVar1 < 0x3130696d) {
      if (iVar1 == 0x3130696c) {
        FUN_00401c00((int)piVar2);
      }
      else {
        if (iVar1 != 0x3030696c) {
          return -1;
        }
        iVar3 = iVar3 + 1;
        FUN_00401be0((int)piVar2);
      }
    }
    else if (iVar1 == 0x3230696c) {
      FUN_00401c20((int)piVar2,param_2);
    }
    else {
      if (iVar1 != 0x3330696c) {
        return -1;
      }
      FUN_00401d20((int)piVar2,param_2,0x43c420);
    }
    piVar2 = (int *)((int)piVar2 + piVar2[1] + 8);
    iVar1 = *piVar2;
  }
  DAT_0043c414 = FUN_00403630(DAT_00452a10,
                              (int)(DAT_0043c460 + ((int)DAT_0043c460 >> 0x1f & 3U)) >> 2);
  if (DAT_0043c414 == (void *)0x0) {
    return -1;
  }
  DAT_0043c464 = FUN_00403630(DAT_00452a10,DAT_0043c460);
  if (DAT_0043c464 == (void *)0x0) {
    return -1;
  }
  DAT_0043c418 = FUN_00403630(DAT_00452a10,DAT_0043c40c);
  if (DAT_0043c418 == (void *)0x0) {
    return -1;
  }
  DAT_0043c408 = FUN_00403630(DAT_00452a10,iVar3 * 0xc);
  if (DAT_0043c408 == (void *)0x0) {
    return -1;
  }
  FUN_00401b60(param_1);
  return iVar3;
}



void __cdecl FUN_00401b60(int *param_1)

{
  int *piVar1;
  int iVar2;
  int *local_4;
  
  if (*param_1 != 0x7878696c) {
    iVar2 = 0;
    piVar1 = local_4;
    do {
      if (*param_1 == 0x3030696c) {
        *(int **)(DAT_0043c408 + iVar2) = param_1 + 2;
        iVar2 = iVar2 + 0xc;
        *(int **)(DAT_0043c408 + -8 + iVar2) = piVar1;
        *(int **)(DAT_0043c408 + -4 + iVar2) = local_4;
      }
      else if (*param_1 == 0x3130696c) {
        piVar1 = param_1 + 2;
        local_4 = (int *)((int)(param_1[1] + (param_1[1] >> 0x1f & 3U)) >> 2);
      }
      param_1 = (int *)((int)param_1 + param_1[1] + 8);
    } while (*param_1 != 0x7878696c);
  }
  return;
}



void __cdecl FUN_00401be0(int param_1)

{
  int iVar1;
  
  iVar1 = *(int *)(param_1 + 4) * 6;
  if (DAT_0043c460 < iVar1) {
    DAT_0043c460 = iVar1;
  }
  return;
}



void __cdecl FUN_00401c00(int param_1)

{
  int iVar1;
  
  iVar1 = *(int *)(param_1 + 4) * 2;
  if (SBORROW4(DAT_0043c40c,iVar1) != DAT_0043c40c + *(int *)(param_1 + 4) * -2 < 0) {
    DAT_0043c40c = iVar1;
  }
  return;
}



undefined4 __cdecl FUN_00401c20(int param_1,int param_2)

{
  int iVar1;
  int iVar2;
  int iVar3;
  ushort *puVar4;
  
  iVar1 = (int)(*(int *)(param_1 + 4) + (*(int *)(param_1 + 4) >> 0x1f & 0xfU)) >> 4;
  DAT_0043c41c = FUN_00403630(DAT_00452a10,iVar1 * 0x1c);
  if (DAT_0043c41c == (void *)0x0) {
    return 0xffffffff;
  }
  if (0 < iVar1) {
    iVar3 = 0;
    puVar4 = (ushort *)(param_1 + 8);
    do {
      *(uint *)((int)DAT_0043c41c + iVar3) = (uint)*puVar4;
      *(uint *)((int)DAT_0043c41c + iVar3 + 4) = (uint)puVar4[1];
      *(uint *)((int)DAT_0043c41c + iVar3 + 8) = (uint)puVar4[2];
      *(uint *)((int)DAT_0043c41c + iVar3 + 0xc) = (uint)puVar4[3];
      *(uint *)((int)DAT_0043c41c + iVar3 + 0x10) = (uint)puVar4[4];
      *(uint *)((int)DAT_0043c41c + iVar3 + 0x14) = (uint)puVar4[5];
      iVar2 = *(int *)(puVar4 + 6);
      if ((0x1e < iVar2) || (iVar2 < 0)) {
        iVar2 = 0;
      }
      *(undefined4 *)((int)DAT_0043c41c + iVar3 + 0x18) = *(undefined4 *)(param_2 + iVar2 * 4);
      iVar3 = iVar3 + 0x1c;
      iVar1 = iVar1 + -1;
      puVar4 = puVar4 + 8;
    } while (iVar1 != 0);
  }
  return 0;
}



undefined4 __cdecl FUN_00401d20(int param_1,int param_2,int param_3)

{
  int iVar1;
  int iVar2;
  short *psVar3;
  
  iVar1 = (int)(*(int *)(param_1 + 4) + (*(int *)(param_1 + 4) >> 0x1f & 0xfU)) >> 4;
  DAT_0043c410 = FUN_00403630(DAT_00452a10,iVar1 << 5);
  if (DAT_0043c410 == (void *)0x0) {
    return 0xffffffff;
  }
  if (0 < iVar1) {
    iVar2 = 0;
    psVar3 = (short *)(param_1 + 8);
    do {
      *(int *)((int)DAT_0043c410 + iVar2) = (int)*psVar3 << 8;
      *(int *)((int)DAT_0043c410 + iVar2 + 4) = (int)psVar3[1] << 8;
      *(uint *)((int)DAT_0043c410 + iVar2 + 8) = (uint)(ushort)psVar3[2] << 8;
      *(uint *)((int)DAT_0043c410 + iVar2 + 0xc) = (uint)(ushort)psVar3[3] << 8;
      *(uint *)((int)DAT_0043c410 + iVar2 + 0x10) = (uint)(ushort)psVar3[4] << 8;
      *(uint *)((int)DAT_0043c410 + iVar2 + 0x14) = (uint)(ushort)psVar3[5] << 8;
      *(undefined4 *)((int)DAT_0043c410 + iVar2 + 0x18) =
           *(undefined4 *)(param_2 + (uint)(ushort)psVar3[6] * 4);
      iVar1 = iVar1 + -1;
      *(undefined4 *)((int)DAT_0043c410 + iVar2 + 0x1c) =
           *(undefined4 *)(param_3 + (uint)(ushort)psVar3[7] * 4);
      iVar2 = iVar2 + 0x20;
      psVar3 = psVar3 + 8;
    } while (iVar1 != 0);
  }
  return 0;
}



int * __cdecl FUN_00401e30(int param_1,int param_2)

{
  byte bVar1;
  short sVar2;
  byte *pbVar3;
  byte *pbVar4;
  undefined4 *puVar5;
  int iVar6;
  uint uVar7;
  short *psVar8;
  uint uVar10;
  int *piVar11;
  uint *puVar12;
  short *psVar9;
  
  puVar5 = (undefined4 *)(param_1 * 0xc + DAT_0043c408);
  pbVar3 = (byte *)*puVar5;
  psVar8 = (short *)puVar5[1];
  iVar6 = puVar5[2];
  if (param_2 == 0) {
    iVar6 = iVar6 * 2;
    piVar11 = DAT_0043c418;
    if (0 < iVar6) {
      do {
        iVar6 = iVar6 + -1;
        *piVar11 = (int)*psVar8 << 4;
        psVar8 = psVar8 + 1;
        piVar11 = piVar11 + 1;
      } while (iVar6 != 0);
    }
  }
  else if ((param_2 == 1) && (piVar11 = DAT_0043c418, 0 < iVar6)) {
    do {
      psVar9 = psVar8 + 1;
      *piVar11 = (int)*psVar8 << 5;
      psVar8 = psVar8 + 2;
      iVar6 = iVar6 + -1;
      piVar11[1] = *psVar9 * 0x26;
      piVar11 = piVar11 + 2;
    } while (iVar6 != 0);
  }
  bVar1 = *pbVar3;
  piVar11 = DAT_0043c414;
  puVar12 = DAT_0043c464;
  while (bVar1 != 0xff) {
    pbVar4 = pbVar3 + 2;
    uVar7 = (uint)*pbVar3;
    uVar10 = (uint)pbVar3[1];
    switch(uVar7) {
    case 7:
      if (param_2 == 0) {
        for (; uVar10 != 0; uVar10 = uVar10 - 1) {
          *piVar11 = (int)puVar12;
          piVar11 = piVar11 + 1;
          *puVar12 = uVar7;
          puVar12[1] = *(short *)pbVar4 * 0x20 + DAT_0043c410;
          puVar12[2] = (uint)(puVar12 + 5);
          puVar12[5] = (uint)*(ushort *)(pbVar4 + 2) << 8;
          puVar12[6] = 0;
          puVar12[7] = 0;
          puVar12[8] = (uint)*(ushort *)(pbVar4 + 4) << 8;
          sVar2 = *(short *)(pbVar4 + 6);
          puVar12[3] = DAT_0043c418[sVar2 * 2];
          puVar12[4] = DAT_0043c418[sVar2 * 2 + 1];
          pbVar4 = pbVar4 + 8;
          puVar12 = puVar12 + 9;
        }
      }
      else if (param_2 == 1) {
        for (; uVar10 != 0; uVar10 = uVar10 - 1) {
          *piVar11 = (int)puVar12;
          piVar11 = piVar11 + 1;
          *puVar12 = uVar7;
          puVar12[1] = *(short *)pbVar4 * 0x20 + DAT_0043c410;
          puVar12[2] = (uint)(puVar12 + 5);
          puVar12[5] = (uint)*(ushort *)(pbVar4 + 2) << 9;
          puVar12[6] = 0;
          puVar12[7] = 0;
          puVar12[8] = (uint)*(ushort *)(pbVar4 + 4) * 0x266;
          sVar2 = *(short *)(pbVar4 + 6);
          puVar12[3] = DAT_0043c418[sVar2 * 2];
          puVar12[4] = DAT_0043c418[sVar2 * 2 + 1];
          pbVar4 = pbVar4 + 8;
          puVar12 = puVar12 + 9;
        }
      }
      break;
    default:
      return (int *)0x0;
    case 0xb:
      for (; uVar10 != 0; uVar10 = uVar10 - 1) {
        *piVar11 = (int)puVar12;
        piVar11 = piVar11 + 1;
        *puVar12 = uVar7;
        sVar2 = *(short *)pbVar4;
        puVar12[1] = DAT_0043c418[sVar2 * 2];
        puVar12[2] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 2);
        puVar12[4] = DAT_0043c418[sVar2 * 2];
        puVar12[5] = DAT_0043c418[sVar2 * 2 + 1];
        puVar12[7] = (uint)pbVar4[4];
        puVar12[8] = 0;
        pbVar4 = pbVar4 + 5;
        puVar12 = puVar12 + 9;
      }
      break;
    case 0xd:
      if (uVar10 != 0) {
        pbVar4 = pbVar4 + uVar10 * 7;
      }
      break;
    case 0xf:
      for (; uVar10 != 0; uVar10 = uVar10 - 1) {
        *piVar11 = (int)puVar12;
        piVar11 = piVar11 + 1;
        *puVar12 = uVar7;
        sVar2 = *(short *)pbVar4;
        puVar12[1] = DAT_0043c418[sVar2 * 2];
        puVar12[2] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 2);
        puVar12[4] = DAT_0043c418[sVar2 * 2];
        puVar12[5] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 4);
        puVar12[7] = DAT_0043c418[sVar2 * 2];
        puVar12[8] = DAT_0043c418[sVar2 * 2 + 1];
        puVar12[10] = (uint)pbVar4[6];
        puVar12[0xb] = 0;
        pbVar4 = pbVar4 + 7;
        puVar12 = puVar12 + 0xc;
      }
      break;
    case 0x11:
      for (; uVar10 != 0; uVar10 = uVar10 - 1) {
        *piVar11 = (int)puVar12;
        piVar11 = piVar11 + 1;
        *puVar12 = uVar7;
        sVar2 = *(short *)pbVar4;
        puVar12[1] = DAT_0043c418[sVar2 * 2];
        puVar12[2] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 2);
        puVar12[3] = DAT_0043c418[sVar2 * 2];
        puVar12[4] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 4);
        puVar12[5] = DAT_0043c418[sVar2 * 2];
        puVar12[6] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 6);
        puVar12[7] = DAT_0043c41c + sVar2 * 0x1c;
        puVar12[8] = *(uint *)(DAT_0043c41c + 0x18 + sVar2 * 0x1c);
        pbVar4 = pbVar4 + 8;
        puVar12 = puVar12 + 9;
      }
      break;
    case 0x12:
      for (; uVar10 != 0; uVar10 = uVar10 - 1) {
        *piVar11 = (int)puVar12;
        piVar11 = piVar11 + 1;
        *puVar12 = uVar7;
        sVar2 = *(short *)pbVar4;
        puVar12[1] = DAT_0043c418[sVar2 * 2];
        puVar12[2] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 2);
        puVar12[3] = DAT_0043c418[sVar2 * 2];
        puVar12[4] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 4);
        puVar12[5] = DAT_0043c418[sVar2 * 2];
        puVar12[6] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 6);
        puVar12[7] = DAT_0043c41c + sVar2 * 0x1c;
        puVar12[8] = *(uint *)(DAT_0043c41c + 0x18 + sVar2 * 0x1c);
        puVar12[9] = (&DAT_0043c420)[pbVar4[8]];
        pbVar4 = pbVar4 + 9;
        puVar12 = puVar12 + 10;
      }
      break;
    case 0x13:
      for (; uVar10 != 0; uVar10 = uVar10 - 1) {
        *piVar11 = (int)puVar12;
        piVar11 = piVar11 + 1;
        *puVar12 = uVar7;
        sVar2 = *(short *)pbVar4;
        puVar12[1] = DAT_0043c418[sVar2 * 2];
        puVar12[2] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 2);
        puVar12[3] = DAT_0043c418[sVar2 * 2];
        puVar12[4] = DAT_0043c418[sVar2 * 2 + 1];
        sVar2 = *(short *)(pbVar4 + 4);
        puVar12[5] = DAT_0043c418[sVar2 * 2];
        puVar12[6] = DAT_0043c418[sVar2 * 2 + 1];
        puVar12[7] = (uint)pbVar4[6];
        puVar12[8] = (&DAT_0043c420)[pbVar4[7]];
        pbVar4 = pbVar4 + 8;
        puVar12 = puVar12 + 9;
      }
    }
    pbVar3 = pbVar4;
    bVar1 = *pbVar4;
  }
  *piVar11 = 0;
  return DAT_0043c414;
}



undefined4 FUN_004023f0(void)

{
  FUN_00402540();
  FUN_004025d0();
  FUN_004027d0();
  FUN_004011a0();
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00402410(void)

{
  int iVar1;
  longlong lVar2;
  
  FUN_00402e30();
  FUN_004012a0();
  if (DAT_0041c548 == 0) {
    DAT_004528bc = *(undefined4 *)(&DAT_004529d0 + DAT_0041c544 * 4);
    iVar1 = FUN_00402840(1.0);
    if (iVar1 == 1) {
      DAT_0041c544 = DAT_0041c544 + 1;
    }
    if (DAT_0041c544 == 3) {
      DAT_0041c548 = DAT_0041c548 + 1;
      _DAT_00452950 = (double)DAT_0041c7b0 * 0.02;
    }
  }
  if ((DAT_0041c548 == 1) || (DAT_0041c548 == 2)) {
    _DAT_00452a08 = (double)DAT_0041c7b0 * 0.02 - _DAT_00452950;
    lVar2 = __ftol();
    FUN_00402aa0((int)lVar2);
  }
  if ((DAT_0041c548 == 3) || (DAT_0041c548 == 4)) {
    DAT_004528bc = *(undefined4 *)(&DAT_004529d0 + DAT_0041c544 * 4);
    iVar1 = FUN_00402840(5.0);
    if (iVar1 == 1) {
      DAT_0041c544 = DAT_0041c544 + 1;
    }
    if (DAT_0041c544 == 4) {
      DAT_0041c7a8 = 2;
    }
  }
  return 0;
}



undefined4 FUN_00402520(void)

{
  FUN_004013a0();
  FUN_00403820(DAT_00452960);
  return 0;
}



void FUN_00402540(void)

{
  DAT_00452960 = FUN_004035a0((char *)0x0);
  DAT_004528b0 = FUN_004037d0(DAT_00452960,0x4b000);
  DAT_004529f8 = FUN_00403630(DAT_00452960,0x1dffff);
  DAT_0045295c = FUN_00403630(DAT_00452960,0x2ffff);
  DAT_00452a00 = FUN_00403630(DAT_00452960,0x20000);
  DAT_004528c0 = FUN_00403630(DAT_00452960,0x20000);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004025d0(void)

{
  int iVar1;
  undefined2 uVar2;
  size_t sVar3;
  undefined1 uVar4;
  char cVar5;
  void *pvVar6;
  int iVar7;
  uint uVar8;
  int iVar9;
  undefined4 *puVar10;
  char *pcVar11;
  undefined4 *puVar12;
  
  iVar9 = 0;
  DAT_004528b8 = FUN_004043a0(s_data_ign_pfm_0041c788);
  _DAT_004528c8 = FUN_004043a0(s_data_ign_psq_0041c778);
  DAT_004528b4 = FUN_004043a0(s_data_ign_col_0041c768);
  DAT_00452948 = FUN_004043a0(s_data_ign0_pic_0041c758);
  _DAT_004529d0 = FUN_004043a0(s_data_ign1_pic_0041c748);
  _DAT_004529d4 = FUN_004043a0(s_data_ign2_pic_0041c738);
  _DAT_004529d8 = FUN_004043a0(s_data_ign4_pic_0041c728);
  _DAT_004529dc = FUN_004043a0(s_data_ign3_pic_0041c718);
  DAT_00452a00 = (void *)((int)DAT_00452a00 + 0xffffU & 0xffff0000);
  pcVar11 = s_data_IGN1_TEX_0041c568;
  FUN_00404320(s_data_ign_pan_0041c708,DAT_00452a00,0x10000,0);
  pvVar6 = (void *)(DAT_004529f8 + 0xffffU & 0xffff0000);
  do {
    sVar3 = FUN_004044d0(pcVar11);
    if ((int)sVar3 < 1) {
                    // WARNING: Subroutine does not return
      _exit(0);
    }
    if (0x100000 < (int)sVar3) {
      sVar3 = 0x100000;
    }
    FUN_00404320(pcVar11,pvVar6,sVar3,0);
    if (0 < (int)sVar3) {
      puVar10 = &DAT_004528d0 + iVar9;
      uVar8 = sVar3 + 0xffff >> 0x10;
      iVar9 = iVar9 + uVar8;
      do {
        *puVar10 = pvVar6;
        puVar10 = puVar10 + 1;
        pvVar6 = (void *)((int)pvVar6 + 0x10000);
        uVar8 = uVar8 - 1;
      } while (uVar8 != 0);
    }
    pcVar11 = pcVar11 + 0x32;
  } while (pcVar11 < s_data_ign_shd_0041c6f8);
  iVar7 = 0;
  (&DAT_004528d0)[iVar9] = 0;
  _DAT_00452970 = (void *)(DAT_0045295c + 0xffffU & 0xffff0000);
  puVar10 = (undefined4 *)((int)_DAT_00452970 + 0x10000);
  FUN_00404320(s_data_ign_shd_0041c6f8,_DAT_00452970,0x10000,0);
  _DAT_00452974 = puVar10;
  do {
    *(char *)puVar10 = (char)iVar7;
    puVar10 = (undefined4 *)((int)puVar10 + 1);
    iVar7 = iVar7 + 1;
  } while (iVar7 < 0x100);
  iVar9 = 1;
  do {
    uVar4 = (undefined1)iVar9;
    iVar9 = iVar9 + 1;
    uVar2 = CONCAT11(uVar4,uVar4);
    puVar12 = puVar10;
    for (iVar7 = 0x40; iVar7 != 0; iVar7 = iVar7 + -1) {
      *puVar12 = CONCAT22(uVar2,uVar2);
      puVar12 = puVar12 + 1;
    }
    puVar10 = puVar10 + 0x40;
  } while (iVar9 < 0x100);
  iVar9 = 0;
  cVar5 = '\0';
  DAT_004528c0 = DAT_004528c0 + 0xffff & 0xffff0000;
  _DAT_00452978 = 0;
  do {
    iVar7 = 0;
    do {
      iVar1 = iVar9 + iVar7;
      iVar7 = iVar7 + 1;
      *(char *)(iVar1 + DAT_004528c0) = cVar5;
    } while (iVar7 < 0x100);
    iVar9 = iVar9 + 0x100;
    cVar5 = cVar5 + '\x01';
  } while (iVar9 < 0x10000);
  return;
}



void FUN_004027d0(void)

{
  FUN_004019d0(DAT_004528b8,0x4528d0,(int *)&DAT_00452970);
  FUN_00402f70(1);
  FUN_004030c0();
  FUN_00402a80(DAT_0041c55c,DAT_0041c558);
  DAT_00452958 = FUN_004044d0(s_data_ign_psq_0041c778);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 __cdecl FUN_00402840(double param_1)

{
  int iVar1;
  
  if (DAT_004528c4 != DAT_004528bc) {
    iVar1 = 0;
    DAT_004528c4 = DAT_004528bc;
    DAT_004529fc = 0;
    _DAT_0041c550 = 0.0;
    FUN_004030c0();
    do {
      iVar1 = iVar1 + 1;
      *(undefined1 *)(DAT_004528b0 + -1 + iVar1) = *(undefined1 *)(DAT_004528bc + 0x34d + iVar1);
    } while (iVar1 < 0x4b000);
    return 0;
  }
  if (DAT_0041c548 < 3) {
    if (((9 < DAT_004529fc) && (DAT_004529fc < 0x28)) && (_DAT_0041c550 < 1.0)) {
      _DAT_0041c550 = _DAT_0041c550 + 0.1;
      FUN_004030c0();
    }
    if ((param_1 * 100.0 < (double)DAT_004529fc) && (0.0 < _DAT_0041c550)) {
      _DAT_0041c550 = _DAT_0041c550 - 0.1;
      if (_DAT_0041c550 < 0.0) {
        _DAT_0041c550 = 0.0;
      }
      FUN_004030c0();
    }
    if (param_1 * 120.0 < (double)DAT_004529fc) {
      return 1;
    }
  }
  if ((DAT_0041c548 == 3) && (_DAT_0041c550 < 1.0)) {
    _DAT_0041c550 = _DAT_0041c550 + 0.1;
    FUN_004030c0();
  }
  if (DAT_0041c548 == 4) {
    if (0.0 < _DAT_0041c550) {
      _DAT_0041c550 = _DAT_0041c550 - 0.1;
      FUN_004030c0();
    }
    if (_DAT_0041c550 <= 0.0) {
      return 1;
    }
  }
  FUN_00404600(DAT_004528b0,DAT_0041c558,0,0,DAT_0041c558,DAT_0041c55c,&DAT_0043c7f8,0,0);
  FUN_004046a0(&DAT_0043c7f8);
  DAT_004529fc = DAT_004529fc + 1;
  return 0;
}



undefined8 __fastcall FUN_00402a80(undefined4 param_1,uint param_2)

{
  longlong lVar1;
  
  lVar1 = FUN_0040a4a0(param_1,param_2);
  return CONCAT44((int)((ulonglong)lVar1 >> 0x20),1);
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 __cdecl FUN_00402aa0(int param_1)

{
  int *piVar1;
  uint extraout_EDX;
  int iVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  
  if (((DAT_0041c548 == 1) && (_DAT_0041c550 < 1.0)) && (4 < DAT_0041c53c)) {
    _DAT_0041c550 = _DAT_0041c550 + 0.1;
    FUN_004030c0();
  }
  if (DAT_0041c548 == 2) {
    _DAT_0041c550 = _DAT_0041c550 - 0.1;
    FUN_004030c0();
    if (_DAT_0041c550 < 0.0) {
      DAT_0041c548 = DAT_0041c548 + 1;
      DAT_0041c560 = 1;
      FUN_00402f70(1);
    }
  }
  piVar1 = (int *)(DAT_0041c538 * 0xc + _DAT_004528c8);
  iVar2 = *piVar1 + DAT_0041c53c;
  iVar7 = piVar1[1];
  DAT_0041c53c = param_1 - _DAT_0041c540;
  if (iVar7 <= param_1 - _DAT_0041c540) {
    _DAT_0041c540 = _DAT_0041c540 + iVar7;
    DAT_0041c53c = 0;
    DAT_0041c538 = DAT_0041c538 + 1;
    if (DAT_00452958 / 0xc == DAT_0041c538) {
      DAT_0041c538 = 0;
    }
  }
  _DAT_00452980 = FUN_00401e30(iVar2,DAT_0041c560);
  _DAT_0045298c = DAT_004528c0;
  _DAT_00452984 = DAT_004528b0;
  _DAT_00452988 = DAT_004528b0;
  _DAT_00452990 = 0;
  _DAT_00452998 = DAT_0041c558 + -1;
  _DAT_00452994 = 0;
  _DAT_0045299c = DAT_0041c55c + -1;
  FUN_00402e10(DAT_004528c0,extraout_EDX);
  if (DAT_0041c560 == 0) {
    FUN_00402f00(0,0,0x14,0x14,0,0,0x140,DAT_0041c558,DAT_00452948 + 0x34e);
    FUN_00402f00(0x14,0,0x14,0x14,DAT_0041c558 + -0x14,0,0x140,DAT_0041c558,DAT_00452948 + 0x34e);
    FUN_00402f00(0,0x14,0x14,0x14,0,DAT_0041c55c + -0x14,0x140,DAT_0041c558,DAT_00452948 + 0x34e);
    iVar2 = DAT_0041c55c + -0x14;
    iVar7 = DAT_0041c558 + -0x14;
    iVar6 = 0x14;
    iVar5 = 0x14;
    iVar4 = 0x14;
    iVar3 = 0x14;
  }
  else {
    FUN_00402f00(0,0x39,0x28,0x28,0,0,0x140,DAT_0041c558,DAT_00452948 + 0x34e);
    FUN_00402f00(0x28,0x39,0x28,0x28,DAT_0041c558 + -0x28,0,0x140,DAT_0041c558,DAT_00452948 + 0x34e)
    ;
    FUN_00402f00(0,0x61,0x28,0x28,0,DAT_0041c55c + -0x28,0x140,DAT_0041c558,DAT_00452948 + 0x34e);
    iVar2 = DAT_0041c55c + -0x28;
    iVar7 = DAT_0041c558 + -0x28;
    iVar6 = 0x28;
    iVar5 = 0x28;
    iVar4 = 0x61;
    iVar3 = 0x28;
  }
  FUN_00402f00(iVar3,iVar4,iVar5,iVar6,iVar7,iVar2,0x140,DAT_0041c558,DAT_00452948 + 0x34e);
  FUN_00402f00(0x28,0,0xad,0x39,(DAT_0041c558 + -0xad) / 2,(DAT_0041c55c * 2) / 200,0x140,
               DAT_0041c558,DAT_00452948 + 0x34e);
  FUN_00404600(DAT_004528b0,DAT_0041c558,0,0,DAT_0041c558,DAT_0041c55c,&DAT_0043c7f8,0,0);
  FUN_004046a0(&DAT_0043c7f8);
  return 0;
}



ulonglong __fastcall FUN_00402e10(undefined4 param_1,uint param_2)

{
  ulonglong uVar1;
  
  uVar1 = FUN_0040a519(param_1,param_2);
  return uVar1;
}



void FUN_00402e30(void)

{
  undefined1 uVar1;
  undefined3 extraout_var;
  undefined3 extraout_var_00;
  undefined3 extraout_var_01;
  undefined3 extraout_var_02;
  undefined3 extraout_var_03;
  
  FUN_00404910();
  uVar1 = FUN_00404a90(0x1c);
  if (CONCAT31(extraout_var,uVar1) != 1) {
    uVar1 = FUN_00404a90(0x39);
    if (CONCAT31(extraout_var_00,uVar1) != 1) {
      uVar1 = FUN_00404a90(1);
      if (CONCAT31(extraout_var_01,uVar1) != 1) goto LAB_00402e7d;
    }
  }
  if (DAT_0041c548 == 1) {
    DAT_0041c548 = 2;
  }
  if (DAT_0041c548 == 3) {
    DAT_0041c548 = 4;
  }
LAB_00402e7d:
  uVar1 = FUN_00404a90(0x4a);
  if ((CONCAT31(extraout_var_02,uVar1) == 1) && (DAT_0041c560 != 0)) {
    FUN_00402f70(0);
    FUN_004030c0();
  }
  uVar1 = FUN_00404a90(0x4e);
  if ((CONCAT31(extraout_var_03,uVar1) == 1) && (DAT_0041c560 != 1)) {
    FUN_00402f70(1);
    FUN_004030c0();
  }
  return;
}



void __cdecl
FUN_00402f00(int param_1,int param_2,int param_3,int param_4,int param_5,int param_6,int param_7,
            int param_8,int param_9)

{
  byte *pbVar1;
  byte *pbVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  
  iVar3 = param_9 + param_7 * param_2 + param_1;
  iVar4 = param_8 * param_6 + param_5 + DAT_004528b0;
  if (0 < param_4) {
    do {
      iVar5 = 0;
      if (0 < param_3) {
        do {
          pbVar1 = (byte *)(iVar3 + iVar5);
          pbVar2 = (byte *)(iVar4 + iVar5);
          iVar5 = iVar5 + 1;
          *(undefined1 *)(iVar4 + -1 + iVar5) =
               *(undefined1 *)((uint)*pbVar1 * 0x100 + (uint)*pbVar2 + DAT_00452a00);
        } while (iVar5 < param_3);
      }
      iVar3 = iVar3 + param_7;
      iVar4 = iVar4 + param_8;
      param_4 = param_4 + -1;
    } while (param_4 != 0);
  }
  return;
}



void __cdecl FUN_00402f70(int param_1)

{
  int iVar1;
  
  DAT_0041c560 = param_1;
  if (param_1 == 0) {
    DAT_0041c558 = 0x140;
    DAT_0041c55c = 200;
  }
  if (param_1 == 1) {
    DAT_0041c558 = 0x280;
    DAT_0041c55c = 0x1e0;
  }
  FUN_004030c0();
  iVar1 = 0;
  do {
    iVar1 = iVar1 + 1;
    *(undefined1 *)(DAT_004528b0 + -1 + iVar1) = 0;
  } while (iVar1 < 0x4b000);
  FUN_00404600(DAT_004528b0,DAT_0041c558,0,0,DAT_0041c558,DAT_0041c55c,&DAT_0043c7f8,0,0);
  FUN_004046a0(&DAT_0043c7f8);
  FUN_00404600(DAT_004528b0,DAT_0041c558,0,0,DAT_0041c558,DAT_0041c55c,&DAT_0043c7f8,0,0);
  DAT_0041c870 = DAT_0041c558;
  DAT_0041c878 = 8;
  DAT_0041c87c = 1;
  DAT_0041c874 = DAT_0041c55c;
  FUN_00404660();
  FUN_004046b0(DAT_004528b0,DAT_0041c558,DAT_0041c558,DAT_0041c55c,8);
  FUN_00402a80(DAT_0041c55c,DAT_0041c558);
  FUN_004030c0();
  return;
}



void FUN_004030c0(void)

{
  int iVar1;
  int iVar2;
  longlong lVar3;
  
  iVar1 = 0;
  do {
    iVar2 = iVar1 + 1;
    lVar3 = __ftol();
    (&DAT_0043c468)[iVar1] = (char)lVar3;
    iVar1 = iVar2;
  } while (iVar2 < 0x300);
  FUN_00404690(&DAT_0043c468);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00403140(HINSTANCE param_1)

{
  ATOM AVar1;
  int iVar2;
  BOOL BVar3;
  tagMSG tStack_44;
  WNDCLASSA WStack_28;
  
  _DAT_0043c7c0 = &DAT_0041b1c0;
  DAT_0043c7b8 = param_1;
  DAT_0043c790 = LoadCursorA((HINSTANCE)0x0,(LPCSTR)0x7f00);
  WStack_28.cbClsExtra = 0;
  WStack_28.cbWndExtra = 0;
  WStack_28.hInstance = DAT_0043c7b8;
  WStack_28.style = 8;
  WStack_28.lpfnWndProc = (WNDPROC)&LAB_00403340;
  WStack_28.hIcon = LoadIconA((HINSTANCE)0x0,(LPCSTR)0x7f00);
  WStack_28.hCursor = DAT_0043c790;
  WStack_28.hbrBackground = (HBRUSH)GetStockObject(4);
  WStack_28.lpszMenuName = (LPCSTR)0x0;
  WStack_28.lpszClassName = s_Ignition_0041c7b8;
  AVar1 = RegisterClassA(&WStack_28);
  if (AVar1 == 0) {
    return 0;
  }
  timeBeginPeriod(1);
  FUN_00404b00();
  iVar2 = FUN_00403510();
  if (iVar2 != 0) {
    while( true ) {
      while (DAT_0043c7a4 != 0) {
        BVar3 = PeekMessageA(&tStack_44,(HWND)0x0,0,0,0);
        if (BVar3 == 0) {
          iVar2 = FUN_004032a0();
          if (iVar2 == 0) {
            FUN_00403540();
          }
        }
        else {
          BVar3 = GetMessageA(&tStack_44,(HWND)0x0,0,0);
          if (BVar3 == 0) {
            return tStack_44.wParam;
          }
          TranslateMessage(&tStack_44);
          DispatchMessageA(&tStack_44);
        }
      }
      BVar3 = GetMessageA(&tStack_44,(HWND)0x0,0,0);
      if (BVar3 == 0) break;
      TranslateMessage(&tStack_44);
      DispatchMessageA(&tStack_44);
    }
    return tStack_44.wParam;
  }
  return 0;
}



undefined4 FUN_004032a0(void)

{
  if ((DAT_0041c7a8 == 0) && (DAT_0041c828 == 0)) {
    DAT_0041c828 = 1;
    DAT_0041c7b0 = FUN_004034d0();
    FUN_004023f0();
    DAT_0041c7a8 = 1;
  }
  else {
    if ((DAT_0041c7a8 == 1) && (DAT_0041c82c == 0)) {
      DAT_0041c7b0 = FUN_004034d0();
      FUN_00402410();
      return 1;
    }
    if ((DAT_0041c7a8 == 2) && (DAT_0041c82c == 0)) {
      DAT_0041c7b0 = FUN_004034d0();
      FUN_00402520();
      FUN_00404b30();
      timeEndPeriod(1);
      DAT_0041c82c = 1;
      return 0;
    }
  }
  return 1;
}



int FUN_004034d0(void)

{
  DWORD DVar1;
  
  DVar1 = timeGetTime();
  if (DAT_0041c830 == 0) {
    DAT_0041c830 = DVar1;
  }
  if (DAT_0043c7a4 != 0) {
    DAT_0041c824 = DAT_0041c824 + (DVar1 - DAT_0041c830);
  }
  DAT_0041c830 = DVar1;
  return DAT_0041c824;
}



undefined4 FUN_00403510(void)

{
  int iVar1;
  
  FUN_004045e0(0);
  iVar1 = FUN_00404640();
  if (iVar1 != 0) {
    iVar1 = FUN_004046f0();
    if (iVar1 != 0) {
      DAT_0043c7b0 = 1;
      return 1;
    }
  }
  return 0;
}



void FUN_00403540(void)

{
  PostMessageA(DAT_0043c7bc,0x10,0,0);
  return;
}



void FUN_00403560(void)

{
  FUN_00404670();
  FUN_00404890();
  return;
}



undefined4 FUN_00403570(void)

{
  int iVar1;
  undefined4 *puVar2;
  
  puVar2 = &DAT_004530d0;
  for (iVar1 = 0x100; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar2 = 0;
    puVar2 = puVar2 + 1;
  }
  FUN_004035a0(s_DEFAULT_0041c83c);
  return 1;
}



int __cdecl FUN_004035a0(char *param_1)

{
  char cVar1;
  int *piVar2;
  undefined1 *puVar3;
  int iVar4;
  int iVar5;
  undefined4 *puVar6;
  
  piVar2 = &DAT_004530d0;
  iVar4 = 0;
  do {
    if (*piVar2 == 0) break;
    piVar2 = piVar2 + 1;
    iVar4 = iVar4 + 1;
  } while (piVar2 < &DAT_004534d0);
  if (iVar4 == 0x100) {
    return -1;
  }
  puVar3 = (undefined1 *)_malloc(0x140);
  if (puVar3 != (undefined1 *)0x0) {
    (&DAT_004530d0)[iVar4] = puVar3;
    if (param_1 == (char *)0x0) {
      *puVar3 = 0;
    }
    else {
      iVar5 = 0;
      cVar1 = *param_1;
      while ((cVar1 != '\0' && (iVar5 < 0x3f))) {
        puVar3[iVar5] = param_1[iVar5];
        iVar5 = iVar5 + 1;
        cVar1 = param_1[iVar5];
      }
      puVar3[iVar5] = 0;
    }
    puVar6 = (undefined4 *)(puVar3 + 0x40);
    for (iVar5 = 0x40; iVar5 != 0; iVar5 = iVar5 + -1) {
      *puVar6 = 0;
      puVar6 = puVar6 + 1;
    }
    return iVar4;
  }
  return -1;
}



void * __cdecl FUN_00403630(int param_1,size_t param_2)

{
  int iVar1;
  int *piVar2;
  int iVar3;
  int *piVar4;
  undefined4 *puVar5;
  undefined4 *puVar6;
  void *pvVar7;
  int *piVar8;
  int iVar9;
  size_t *psVar10;
  int *piVar11;
  int iVar12;
  int iVar13;
  int iVar14;
  
  iVar9 = (&DAT_004530d0)[param_1];
  piVar8 = (int *)(iVar9 + 0x40);
  iVar1 = *piVar8;
  for (iVar14 = 0; (iVar1 != 0 && (iVar14 < 0x40)); iVar14 = iVar14 + 1) {
    piVar2 = (int *)*piVar8;
    iVar1 = *piVar2;
    piVar11 = piVar2;
    for (iVar12 = 0; (iVar1 != 0 && (iVar12 < 0x40)); iVar12 = iVar12 + 1) {
      iVar1 = *piVar11;
      piVar4 = (int *)(iVar1 + 4);
      iVar3 = *piVar4;
      for (iVar13 = 0; (iVar3 != 0 && (iVar13 < 0x10)); iVar13 = iVar13 + 1) {
        piVar4 = piVar4 + 2;
        iVar3 = *piVar4;
      }
      if (iVar13 != 0x10) {
        pvVar7 = _malloc(param_2);
        if (pvVar7 != (void *)0x0) {
          puVar5 = (undefined4 *)(iVar1 + iVar13 * 8);
          *puVar5 = pvVar7;
          puVar5[1] = param_2;
          return pvVar7;
        }
        return (void *)0x0;
      }
      piVar11 = piVar11 + 1;
      iVar1 = *piVar11;
    }
    if (iVar12 != 0x40) {
      puVar5 = (undefined4 *)_malloc(0x80);
      if (puVar5 == (undefined4 *)0x0) {
        return (void *)0x0;
      }
      puVar6 = puVar5 + 3;
      piVar2[iVar12] = (int)puVar5;
      iVar9 = 0xf;
      do {
        *puVar6 = 0;
        puVar6 = puVar6 + 2;
        iVar9 = iVar9 + -1;
      } while (iVar9 != 0);
      pvVar7 = _malloc(param_2);
      if (pvVar7 != (void *)0x0) {
        *puVar5 = pvVar7;
        puVar5[1] = param_2;
        return pvVar7;
      }
      return (void *)0x0;
    }
    piVar8 = piVar8 + 1;
    iVar1 = *piVar8;
  }
  if (iVar14 == 0x40) {
    return (void *)0x0;
  }
  puVar5 = (undefined4 *)_malloc(0x100);
  if (puVar5 == (undefined4 *)0x0) {
    return (void *)0x0;
  }
  *(undefined4 **)(iVar9 + 0x40 + iVar14 * 4) = puVar5;
  puVar6 = puVar5;
  for (iVar9 = 0x40; iVar9 != 0; iVar9 = iVar9 + -1) {
    *puVar6 = 0;
    puVar6 = puVar6 + 1;
  }
  puVar6 = (undefined4 *)_malloc(0x80);
  if (puVar6 == (undefined4 *)0x0) {
    return (void *)0x0;
  }
  iVar9 = 0x10;
  *puVar5 = puVar6;
  psVar10 = puVar6 + 1;
  do {
    *psVar10 = 0;
    psVar10 = psVar10 + 2;
    iVar9 = iVar9 + -1;
  } while (iVar9 != 0);
  pvVar7 = _malloc(param_2);
  if (pvVar7 != (void *)0x0) {
    *puVar6 = pvVar7;
    puVar6[1] = param_2;
    return pvVar7;
  }
  return (void *)0x0;
}



undefined4 * __cdecl FUN_004037d0(int param_1,uint param_2)

{
  undefined4 *puVar1;
  uint uVar2;
  undefined4 *puVar3;
  
  puVar1 = (undefined4 *)FUN_00403630(param_1,param_2);
  if (puVar1 != (undefined4 *)0x0) {
    puVar3 = puVar1;
    for (uVar2 = param_2 >> 2; uVar2 != 0; uVar2 = uVar2 - 1) {
      *puVar3 = 0;
      puVar3 = puVar3 + 1;
    }
    for (uVar2 = param_2 & 3; uVar2 != 0; uVar2 = uVar2 - 1) {
      *(undefined1 *)puVar3 = 0;
      puVar3 = (undefined4 *)((int)puVar3 + 1);
    }
  }
  return puVar1;
}



undefined4 __cdecl FUN_00403820(int param_1)

{
  void *_Memory;
  undefined4 *_Memory_00;
  void *_Memory_01;
  undefined4 *puVar1;
  int iVar2;
  int *piVar3;
  undefined4 *local_10;
  int local_8;
  int local_4;
  
  _Memory = (void *)(&DAT_004530d0)[param_1];
  local_10 = (undefined4 *)((int)_Memory + 0x40);
  local_4 = 0x40;
  do {
    _Memory_00 = (undefined4 *)*local_10;
    if (_Memory_00 != (undefined4 *)0x0) {
      local_8 = 0x40;
      puVar1 = _Memory_00;
      do {
        _Memory_01 = (void *)*puVar1;
        if (_Memory_01 != (void *)0x0) {
          piVar3 = (int *)((int)_Memory_01 + 4);
          iVar2 = 0x10;
          do {
            if (*piVar3 != 0) {
              _free((void *)piVar3[-1]);
            }
            piVar3 = piVar3 + 2;
            iVar2 = iVar2 + -1;
          } while (iVar2 != 0);
          _free(_Memory_01);
        }
        puVar1 = puVar1 + 1;
        local_8 = local_8 + -1;
      } while (local_8 != 0);
      _free(_Memory_00);
    }
    local_10 = local_10 + 1;
    local_4 = local_4 + -1;
  } while (local_4 != 0);
  _free(_Memory);
  (&DAT_004530d0)[param_1] = 0;
  return 1;
}



undefined4 FUN_004038e0(void)

{
  int iVar1;
  
  iVar1 = 0;
  do {
    if ((&DAT_004530d0)[iVar1] != 0) {
      FUN_00403820(iVar1);
    }
    iVar1 = iVar1 + 1;
  } while (iVar1 < 0x100);
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined * FUN_00403910(void)

{
  if (DAT_0041c848 != 1) {
    return (undefined *)0x0;
  }
  if (DAT_0043c7d8 != 1) {
    if (DAT_0041c844 == 0) {
      (**(code **)(*DAT_004530a0 + 0x30))(DAT_004530a0,0,0,1);
      DAT_0041c844 = 1;
    }
    (**(code **)(*DAT_004530a0 + 0x10))(DAT_004530a0,&DAT_0043c7e4,&DAT_0043c7cc);
    DAT_0043c7cc = DAT_0043c7e4 + DAT_0043c7c4;
    if (DAT_0043c7f4 <= DAT_0043c7cc) {
      DAT_0043c7cc = DAT_0043c7cc - DAT_0043c7f4;
    }
    DAT_0043c7c8 = DAT_0043c7cc + DAT_0045308c * DAT_0043c7f0;
    if (DAT_0043c7c8 < DAT_0043c7f4) {
      if ((DAT_0043c7e0 < DAT_0043c7c8) && (DAT_0043c7cc <= DAT_0043c7e0)) {
        return (undefined *)0x0;
      }
    }
    else {
      DAT_0043c7c8 = DAT_0043c7c8 - DAT_0043c7f4;
      if ((DAT_0043c7e0 < DAT_0043c7c8) || (DAT_0043c7cc <= DAT_0043c7e0)) {
        return (undefined *)0x0;
      }
    }
    DAT_0043c7dc = (**(code **)(*DAT_004530a0 + 0x2c))
                             (DAT_004530a0,DAT_0043c7e0,DAT_0045308c * DAT_0043c7f0,&DAT_0043c7d0,
                              &DAT_0043c7e8,&DAT_0043c7d4,&DAT_0043c7ec,0);
    if (DAT_0043c7dc == -0x7787ff6a) {
      (**(code **)(*DAT_004530a0 + 0x50))(DAT_004530a0);
      (**(code **)(*DAT_004530a0 + 0x30))(DAT_004530a0,0,0,1);
      return (undefined *)0x0;
    }
    if (DAT_0043c7dc != 0) {
      return (undefined *)0x0;
    }
    _DAT_004530b0 = DAT_0043c7d0;
    _DAT_004530b4 = DAT_0043c7d4;
    _DAT_004530b8 = DAT_0043c7e8 / DAT_0043c7f0;
    _DAT_004530bc = DAT_0043c7ec / DAT_0043c7f0;
    return &DAT_004530b0;
  }
  if (DAT_0041c844 == 0) {
    (**(code **)(*DAT_004530c0 + 0x30))(DAT_004530c0,0,0,1);
    DAT_0041c844 = 1;
  }
  (**(code **)(*DAT_004530c0 + 0x10))(DAT_004530c0,&DAT_0043c7e4,&DAT_0043c7cc);
  DAT_0043c7cc = DAT_0043c7e4 + DAT_0043c7c4;
  if (DAT_0043c7f4 <= DAT_0043c7cc) {
    DAT_0043c7cc = DAT_0043c7cc - DAT_0043c7f4;
  }
  DAT_0043c7c8 = DAT_0043c7cc + DAT_0045308c * DAT_0043c7f0;
  if (DAT_0043c7c8 < DAT_0043c7f4) {
    if ((DAT_0043c7e0 < DAT_0043c7c8) && (DAT_0043c7cc <= DAT_0043c7e0)) {
      return (undefined *)0x0;
    }
  }
  else {
    DAT_0043c7c8 = DAT_0043c7c8 - DAT_0043c7f4;
    if ((DAT_0043c7e0 < DAT_0043c7c8) || (DAT_0043c7cc <= DAT_0043c7e0)) {
      return (undefined *)0x0;
    }
  }
  DAT_0043c7dc = (**(code **)(*DAT_004530c0 + 0x2c))
                           (DAT_004530c0,DAT_0043c7e0,DAT_0045308c * DAT_0043c7f0,&DAT_0043c7d0,
                            &DAT_0043c7e8,&DAT_0043c7d4,&DAT_0043c7ec,0);
  if (DAT_0043c7dc == -0x7787ff6a) {
    (**(code **)(*DAT_004530c0 + 0x50))(DAT_004530c0);
    (**(code **)(*DAT_004530c0 + 0x30))(DAT_004530c0,0,0,1);
    return (undefined *)0x0;
  }
  if (DAT_0043c7dc != 0) {
    return (undefined *)0x0;
  }
  _DAT_004530b0 = DAT_0043c7d0;
  _DAT_004530b4 = DAT_0043c7d4;
  _DAT_004530b8 = DAT_0043c7e8 / DAT_0043c7f0;
  _DAT_004530bc = DAT_0043c7ec / DAT_0043c7f0;
  return &DAT_004530b0;
}



undefined4 FUN_00403bf0(void)

{
  if (DAT_0041c848 != 1) {
    return 0;
  }
  if (DAT_0043c7d8 != 1) {
    (**(code **)(*DAT_004530a0 + 0x4c))
              (DAT_004530a0,DAT_0043c7d0,DAT_0043c7e8,DAT_0043c7d4,DAT_0043c7ec);
    for (DAT_0043c7e0 = DAT_0043c7e0 + DAT_0045308c * DAT_0043c7f0; DAT_0043c7f4 <= DAT_0043c7e0;
        DAT_0043c7e0 = DAT_0043c7e0 - DAT_0043c7f4) {
    }
    return 0;
  }
  (**(code **)(*DAT_004530c0 + 0x4c))(DAT_004530c0);
  for (DAT_0043c7e0 = DAT_0043c7e0 + DAT_0045308c * DAT_0043c7f0; DAT_0043c7f4 <= DAT_0043c7e0;
      DAT_0043c7e0 = DAT_0043c7e0 - DAT_0043c7f4) {
  }
  return 0;
}



undefined4 FUN_00403cb0(void)

{
  if (DAT_0041c848 != 1) {
    return 0;
  }
  DAT_0041c848 = 0;
  if (DAT_00453070 != (int *)0x0) {
    if (DAT_004530a0 != (int *)0x0) {
      (**(code **)(*DAT_004530a0 + 8))(DAT_004530a0);
      DAT_004530a0 = (int *)0x0;
    }
    if (DAT_004530c0 != (int *)0x0) {
      (**(code **)(*DAT_004530c0 + 8))(DAT_004530c0);
      DAT_004530c0 = (int *)0x0;
    }
    (**(code **)(*DAT_00453070 + 8))(DAT_00453070);
    DAT_00453070 = (int *)0x0;
  }
  return 0;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00403d20(void)

{
  uint uVar1;
  undefined2 uVar2;
  int iVar3;
  uint uVar4;
  int iVar5;
  uint uVar6;
  int iVar7;
  uint *puVar8;
  undefined4 uStack_11c;
  undefined4 *puStack_118;
  int iStack_114;
  undefined4 uStack_110;
  uint uStack_10c;
  int iStack_108;
  undefined4 uStack_104;
  undefined4 *puStack_100;
  int iStack_fc;
  undefined4 uStack_e8;
  uint auStack_e4 [57];
  
  auStack_e4[0xb] = 2;
  auStack_e4[0xe] = 2;
  auStack_e4[0xf] = 2;
  auStack_e4[0x10] = 0x5622;
  auStack_e4[0x11] = 0x5622;
  auStack_e4[0x13] = 0x5622;
  auStack_e4[0x12] = 0xac44;
  auStack_e4[0x14] = 0x5622;
  auStack_e4[0x15] = 0xac44;
  auStack_e4[0x16] = 0x10;
  auStack_e4[0x17] = 0x10;
  auStack_e4[10] = 1;
  auStack_e4[0xc] = 1;
  auStack_e4[0xd] = 1;
  auStack_e4[0x18] = 0x10;
  auStack_e4[0x19] = 8;
  auStack_e4[0x1a] = 8;
  auStack_e4[0x1b] = 8;
  if (DAT_0041c848 != 1) {
    _DAT_00453094 = 4;
    DAT_004530c0 = (int *)0x0;
    DAT_004530a0 = (int *)0x0;
    DAT_0043c7d8 = 0;
    DAT_0041c848 = 0;
    iStack_fc = 0;
    puStack_100 = &DAT_00453070;
    uStack_104 = 0;
    iStack_108 = 0x403de7;
    iVar3 = DirectSoundCreate();
    if (iVar3 == 0) {
      iStack_108 = 4;
      uStack_10c = DAT_0041c7ac;
      uStack_110 = DAT_00453070;
      iStack_114 = 0x403e03;
      iVar3 = (**(code **)(*DAT_00453070 + 0x18))();
      if (iVar3 == 0) {
        DAT_0043c7d8 = 1;
        auStack_e4[2] = 0;
        uStack_e8 = 0x14;
        uStack_11c = &uStack_e8;
        iStack_114 = 0;
        puStack_118 = &DAT_004530c0;
        auStack_e4[0] = 1;
        auStack_e4[1] = 0;
        auStack_e4[3] = 0;
        iVar3 = (**(code **)(*DAT_00453070 + 0xc))(DAT_00453070);
        if (iVar3 == 0) {
          iVar3 = 0;
          do {
            uVar6 = auStack_e4[iVar3];
            uVar1 = auStack_e4[iVar3 + 0xc];
            uStack_10c = auStack_e4[iVar3 + 6];
            uStack_110 = (int *)CONCAT22((short)uVar6,1);
            iVar7 = uVar6 - 1;
            uVar6 = (int)(uVar1 * uVar6 + ((int)(uVar1 * uVar6) >> 0x1f & 7U)) >> 3;
            uStack_104 = CONCAT22((short)uVar1,(short)uVar6);
            uVar4 = uStack_10c / 100;
            DAT_0043c7f0 = uVar6 & 0xffff;
            iStack_108 = uStack_10c * DAT_0043c7f0;
            iVar5 = (**(code **)(*DAT_004530c0 + 0x38))(DAT_004530c0,&uStack_110);
            DAT_00453080 = auStack_e4[iVar3 + 6];
            DAT_00453090 = ((int)(uVar1 + ((int)uVar1 >> 0x1f & 7U)) >> 3) + -1;
            if (iVar5 == 0) {
              iVar3 = 6;
            }
            iVar3 = iVar3 + 1;
            DAT_00453084 = uVar1;
            DAT_00453088 = iVar7;
          } while (iVar3 < 6);
          if (iVar5 == 0) {
            auStack_e4[0x12] = 0x14;
            iVar3 = (**(code **)(*DAT_004530c0 + 0xc))(DAT_004530c0,auStack_e4 + 0x12);
            if (iVar3 == 0) {
              DAT_0043c7f4 = auStack_e4[0x12];
              DAT_0043c7c4 = DAT_00453098 * uVar4 * DAT_0043c7f0;
              iVar3 = auStack_e4[0x12] - uVar4 * DAT_0043c7f0;
              if (iVar3 < DAT_0043c7c4) {
                DAT_0043c7c4 = iVar3;
              }
              DAT_0043c7e0 = 0;
              DAT_0045308c = uVar4;
              (**(code **)(*DAT_004530c0 + 0x30))(DAT_004530c0,0,0,1);
              DAT_0041c844 = 1;
              DAT_0041c848 = 1;
              return 1;
            }
          }
        }
      }
      else {
        iStack_114 = 3;
        puStack_118 = (undefined4 *)DAT_0041c7ac;
        uStack_11c = DAT_00453070;
        iVar3 = (**(code **)(*DAT_00453070 + 0x18))();
        if (iVar3 == 0) {
          uStack_e8 = 0;
          auStack_e4[0] = 0;
          iVar3 = (**(code **)(*DAT_00453070 + 0xc))(DAT_00453070,&stack0xffffff0c,&DAT_004530c0,0);
          if (iVar3 == 0) {
            iVar3 = 0;
            do {
              uVar6 = auStack_e4[iVar3 + 9];
              uVar1 = auStack_e4[iVar3 + 3];
              uStack_11c = (int *)CONCAT22((short)auStack_e4[iVar3 + -3],1);
              uVar4 = (int)(uVar6 * auStack_e4[iVar3 + -3] +
                           ((int)(uVar6 * auStack_e4[iVar3 + -3]) >> 0x1f & 7U)) >> 3;
              uVar2 = (undefined2)uVar4;
              iStack_108 = CONCAT22(iStack_108._2_2_,uVar2);
              uStack_110 = (int *)CONCAT22((short)uVar6,uVar2);
              DAT_0043c7f0 = uVar4 & 0xffff;
              uStack_10c = uVar1 / 100;
              iVar7 = DAT_0043c7f0 * uVar1;
              puStack_118 = (undefined4 *)uVar1;
              iStack_114 = iVar7;
              iVar5 = (**(code **)(*DAT_004530c0 + 0x38))(DAT_004530c0,&uStack_11c);
              DAT_00453088 = auStack_e4[iVar3 + -3] - 1;
              DAT_00453090 = ((int)(uVar6 + ((int)uVar6 >> 0x1f & 7U)) >> 3) + -1;
              DAT_00453080 = uVar1;
              DAT_00453084 = uVar6;
              if (iVar5 == 0) {
                uStack_110 = (int *)CONCAT22((short)uVar6,(short)iStack_108);
                uStack_104 = 0x14;
                puStack_100 = (undefined4 *)0xe8;
                uStack_11c = (int *)CONCAT22((short)auStack_e4[iVar3 + -3],1);
                DAT_0043c7f4 = iVar7;
                puStack_118 = (undefined4 *)uVar1;
                iStack_114 = iVar7;
                iStack_fc = iVar7;
                iVar3 = (**(code **)(*DAT_00453070 + 0xc))(DAT_00453070,&uStack_104,&DAT_004530a0,0)
                ;
                if (iVar3 != 0) {
                  return 0;
                }
                iVar3 = 6;
                iVar5 = 0;
              }
              iVar3 = iVar3 + 1;
            } while (iVar3 < 6);
            if (iVar5 == 0) {
              DAT_0045308c = uStack_10c;
              auStack_e4[0xf] = 0x14;
              iVar3 = (**(code **)(*DAT_004530a0 + 0xc))(DAT_004530a0,auStack_e4 + 0xf);
              if (iVar3 == 0) {
                DAT_0043c7f4 = auStack_e4[0xf];
                DAT_0043c7c4 = (DAT_00453098 + 0x1b) * iStack_114 * DAT_0043c7f0;
                puVar8 = auStack_e4 + 0x12;
                for (iVar3 = 0x18; iVar3 != 0; iVar3 = iVar3 + -1) {
                  *puVar8 = 0;
                  puVar8 = puVar8 + 1;
                }
                auStack_e4[0x12] = 0x60;
                iVar3 = (**(code **)(*DAT_00453070 + 0x10))(DAT_00453070,auStack_e4 + 0x12);
                if ((iVar3 == 0) && ((auStack_e4[0x11] & 0x20) == 0)) {
                  DAT_0043c7c4 = (DAT_00453098 + 2) * (int)uStack_11c * DAT_0043c7f0;
                }
                iVar3 = DAT_0043c7f4 - (int)uStack_11c * DAT_0043c7f0;
                if (iVar3 < DAT_0043c7c4) {
                  DAT_0043c7c4 = iVar3;
                }
                DAT_0043c7e0 = 0;
                (**(code **)(*DAT_004530c0 + 0x30))(DAT_004530c0,0,0,1);
                (**(code **)(*DAT_004530a0 + 0x30))(DAT_004530a0,0,0,1);
                DAT_0041c844 = 1;
                DAT_0041c848 = 1;
                return 1;
              }
            }
          }
        }
      }
    }
  }
  return 0;
}



undefined4 __cdecl FUN_00404320(char *param_1,void *param_2,size_t param_3,int param_4)

{
  FILE *_File;
  size_t sVar1;
  int local_8;
  int local_4;
  
  _File = (FILE *)FUN_004114b0(param_1,&DAT_0041c85c);
  if (_File == (FILE *)0x0) {
    return 2000;
  }
  local_4 = param_4 >> 0x1f;
  local_8 = param_4;
  _fsetpos(_File,(fpos_t *)&local_8);
  sVar1 = _fread(param_2,1,param_3,_File);
  if (param_3 != sVar1) {
    return 0x7da;
  }
  _fclose(_File);
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void * __cdecl FUN_004043a0(char *param_1)

{
  int iVar1;
  size_t _Count;
  void *_DstBuf;
  FILE *_File;
  size_t sVar2;
  undefined4 local_8;
  undefined4 local_4;
  
  iVar1 = FUN_00404500(param_1);
  if (iVar1 != 1) {
    _DAT_0041c90c = 0x7ee;
    return (void *)0x0;
  }
  _Count = FUN_004044d0(param_1);
  if (_Count == 0) {
    _DAT_0041c90c = 0x7f8;
    return (void *)0x0;
  }
  _DstBuf = FUN_00403630(0,_Count);
  if (_DstBuf == (void *)0x0) {
    _DAT_0041c90c = 0x802;
    return (void *)0x0;
  }
  _File = (FILE *)FUN_004114b0(param_1,&DAT_0041c85c);
  if (_File == (FILE *)0x0) {
    _DAT_0041c90c = 2000;
    return (void *)0x0;
  }
  local_8 = 0;
  local_4 = 0;
  _fsetpos(_File,(fpos_t *)&local_8);
  sVar2 = _fread(_DstBuf,1,_Count,_File);
  if (_Count != sVar2) {
    _DAT_0041c90c = 0x7da;
    return (void *)0x0;
  }
  _fclose(_File);
  return _DstBuf;
}



long __cdecl FUN_00404490(FILE *param_1)

{
  long _Offset;
  long lVar1;
  
  _Offset = _ftell(param_1);
  _fseek(param_1,0,2);
  lVar1 = _ftell(param_1);
  _fseek(param_1,_Offset,0);
  return lVar1;
}



long __cdecl FUN_004044d0(char *param_1)

{
  FILE *_File;
  long lVar1;
  
  _File = (FILE *)FUN_004114b0(param_1,&DAT_0041c860);
  lVar1 = FUN_00404490(_File);
  _fclose(_File);
  return lVar1;
}



undefined4 __cdecl FUN_00404500(char *param_1)

{
  FILE *_File;
  
  _File = (FILE *)FUN_004114b0(param_1,&DAT_0041c860);
  if (_File == (FILE *)0x0) {
    return 0x7ef;
  }
  _fclose(_File);
  return 1;
}



undefined4 FUN_00404530(void)

{
  undefined4 *puVar1;
  undefined4 *puVar2;
  
  puVar2 = &DAT_0043c8e8;
  do {
    *puVar2 = 0;
    puVar1 = puVar2 + 0xc;
    puVar2[1] = 0;
    puVar2[2] = 0;
    puVar2[3] = 0;
    puVar2[4] = 0;
    puVar2[5] = 0;
    puVar2[6] = 0;
    puVar2[7] = 0;
    puVar2[8] = 0;
    puVar2[9] = 0;
    puVar2[10] = 0;
    puVar2[0xb] = 0;
    puVar2 = puVar1;
  } while (puVar1 < &DAT_0043c918);
  puVar2 = &DAT_0043c7f8;
  do {
    *puVar2 = 0;
    puVar1 = puVar2 + 0xc;
    puVar2[1] = 0;
    puVar2[2] = 0;
    puVar2[3] = 0;
    puVar2[4] = 0;
    puVar2[5] = 0;
    puVar2[6] = 0;
    puVar2[7] = 0;
    puVar2[8] = 0;
    puVar2[9] = 0;
    puVar2[10] = 1;
    puVar2[0xb] = 0;
    puVar2 = puVar1;
  } while (puVar1 < &DAT_0043c8e8);
  puVar2 = &DAT_0043c918;
  do {
    *puVar2 = 0;
    puVar1 = puVar2 + 0xc;
    puVar2[1] = 0;
    puVar2[2] = 0;
    puVar2[3] = 0;
    puVar2[4] = 0;
    puVar2[5] = 0;
    puVar2[6] = 0;
    puVar2[7] = 0;
    puVar2[8] = 0;
    puVar2[9] = 0;
    puVar2[10] = 2;
    puVar2[0xb] = 0;
    puVar2 = puVar1;
  } while (puVar1 < &DAT_0043ccd8);
  return 1;
}



undefined4 __cdecl FUN_004045e0(int param_1)

{
  if (param_1 == 0) {
    FUN_00405220();
    FUN_00404c60();
    return 1;
  }
  return 2;
}



void __cdecl
FUN_00404600(undefined4 param_1,undefined4 param_2,undefined4 param_3,undefined4 param_4,
            undefined4 param_5,undefined4 param_6,undefined4 param_7,undefined4 param_8,
            undefined4 param_9)

{
  (*DAT_0043ccf0)(param_1,param_2,param_3,param_4,param_5,param_6,param_7,param_8,param_9);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00404640(void)

{
  int iVar1;
  undefined4 uVar2;
  
  iVar1 = (*DAT_0043ccdc)();
  if (iVar1 == 0) {
    return 0;
  }
                    // WARNING: Could not recover jumptable at 0x0040464d. Too many branches
                    // WARNING: Treating indirect jump as call
  uVar2 = (*_DAT_0043cd10)();
  return uVar2;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00404660(void)

{
                    // WARNING: Could not recover jumptable at 0x00404660. Too many branches
                    // WARNING: Treating indirect jump as call
  (*_DAT_0043cce0)();
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00404670(void)

{
  int iVar1;
  undefined4 uVar2;
  
  iVar1 = (*DAT_0043cce4)();
  if (iVar1 == 0) {
    return 0;
  }
                    // WARNING: Could not recover jumptable at 0x0040467d. Too many branches
                    // WARNING: Treating indirect jump as call
  uVar2 = (*_DAT_0043cd14)();
  return uVar2;
}



void __cdecl FUN_00404690(undefined4 param_1)

{
  (*DAT_0043cd08)(param_1);
  return;
}



void __cdecl FUN_004046a0(undefined4 param_1)

{
  (*DAT_0043ccf8)(param_1);
  return;
}



void __cdecl
FUN_004046b0(undefined4 param_1,undefined4 param_2,undefined4 param_3,undefined4 param_4,
            undefined4 param_5)

{
  (*DAT_0043cd20)(param_1,param_2,param_3,param_4,param_5);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004046e0(void)

{
  DAT_0043cd64 = 0;
  _DAT_0043d5c8 = 0;
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_004046f0(void)

{
  int iVar1;
  int iVar2;
  undefined4 *puVar3;
  undefined4 uStack_58;
  undefined4 uStack_54;
  int *piStack_50;
  undefined *puStack_4c;
  int *piStack_48;
  undefined4 *puStack_44;
  undefined4 *puStack_40;
  undefined4 uStack_3c;
  undefined4 uStack_38;
  undefined4 uStack_34;
  undefined4 *puStack_30;
  undefined4 uStack_2c;
  undefined4 local_20;
  undefined4 local_1c;
  undefined4 local_18;
  undefined4 local_14;
  undefined4 local_10;
  undefined4 local_c;
  undefined4 local_8;
  undefined4 local_4;
  
  local_10 = 0x6f1d2b61;
  local_20 = 0x10;
  local_c = 0x11cfd5a0;
  local_8 = 0x4544c7bf;
  local_4 = 0x5453;
  uStack_2c = 0;
  local_1c = 0;
  puStack_30 = &DAT_0043ceb0;
  local_18 = 0;
  uStack_34 = 0x300;
  local_14 = 0x20;
  uStack_38 = DAT_0043c7b8;
  uStack_3c = 0x40474f;
  iVar1 = DirectInputCreateA();
  if (iVar1 != 0) {
    return 0;
  }
  puStack_44 = &local_20;
  uStack_3c = 0;
  puStack_40 = &DAT_0043d1bc;
  piStack_48 = DAT_0043ceb0;
  puStack_4c = (undefined *)0x404772;
  iVar1 = (**(code **)(*DAT_0043ceb0 + 0xc))();
  if (iVar1 != 0) {
    return 0;
  }
  puStack_4c = &DAT_0040a480;
  piStack_50 = DAT_0043d1bc;
  uStack_54 = 0x40478d;
  iVar1 = (**(code **)(*DAT_0043d1bc + 0x2c))();
  if (iVar1 != 0) {
    return 0;
  }
  uStack_54 = 6;
  uStack_58 = DAT_0043c7bc;
  iVar1 = (**(code **)(*DAT_0043d1bc + 0x34))(DAT_0043d1bc);
  if (iVar1 != 0) {
    return 0;
  }
  iVar1 = (**(code **)(*DAT_0043d1bc + 0x18))(DAT_0043d1bc,1,&uStack_58);
  if (iVar1 != 0) {
    return 0;
  }
  iVar1 = (**(code **)(*DAT_0043d1bc + 0x1c))(DAT_0043d1bc);
  DAT_0043d1c0 = (uint)(-1 < iVar1);
  iVar2 = 0;
  DAT_0043cd60 = &DAT_0043cd68;
  puVar3 = &DAT_0043cd68;
  for (iVar1 = 0x10; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar3 = 0xffffffff;
    puVar3 = puVar3 + 1;
  }
  DAT_0043cda8 = 0;
  puVar3 = (undefined4 *)&DAT_0043ceb8;
  for (iVar1 = 0x40; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar3 = 0;
    puVar3 = puVar3 + 1;
  }
  puVar3 = (undefined4 *)&DAT_0043cdb0;
  for (iVar1 = 0x40; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar3 = 0;
    puVar3 = puVar3 + 1;
  }
  do {
    (&DAT_0043cfb8)[iVar2] = 0;
    (&DAT_0043d1c8)[iVar2] = 0;
    (&DAT_0043d0b8)[iVar2] = 0;
    iVar2 = iVar2 + 1;
  } while (iVar2 < 0x100);
  DAT_0043ceb4 = puStack_40;
  _DAT_0043d1c4 = uStack_3c;
  _DAT_0043d1b8 = DAT_0041c7b0;
  DAT_0041c894 = 0;
  DAT_0041c890 = 0;
  return 1;
}



undefined4 FUN_00404890(void)

{
  if (DAT_0043d1c0 != 0) {
    (**(code **)(*DAT_0043d1bc + 0x20))(DAT_0043d1bc);
    DAT_0043d1c0 = 0;
  }
  if (DAT_0043d1bc != (int *)0x0) {
    (**(code **)(*DAT_0043d1bc + 8))(DAT_0043d1bc);
    DAT_0043d1bc = (int *)0x0;
  }
  if (DAT_0043ceb0 != (int *)0x0) {
    (**(code **)(*DAT_0043ceb0 + 8))(DAT_0043ceb0);
    DAT_0043ceb0 = (int *)0x0;
  }
  if (DAT_0041c894 != 0) {
    timeKillEvent(DAT_0041c890);
  }
  return 1;
}



// WARNING: Removing unreachable block (ram,0x004049de)
// WARNING: Removing unreachable block (ram,0x004049e7)
// WARNING: Removing unreachable block (ram,0x00404a3c)
// WARNING: Removing unreachable block (ram,0x004049f3)
// WARNING: Removing unreachable block (ram,0x00404a02)
// WARNING: Removing unreachable block (ram,0x00404a10)
// WARNING: Removing unreachable block (ram,0x00404a19)
// WARNING: Removing unreachable block (ram,0x00404a20)
// WARNING: Removing unreachable block (ram,0x00404a62)
// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00404910(void)

{
  int iVar1;
  int iVar2;
  int *piVar3;
  int iVar4;
  undefined4 local_204;
  undefined1 local_200 [512];
  
  iVar1 = 0;
  piVar3 = &DAT_0043d1c8;
  local_204 = 0x20;
  do {
    if ((&DAT_0043cdb0)[iVar1] == '\x01') {
      iVar4 = DAT_0043ceb4 + _DAT_0043d1c4;
      iVar2 = (*piVar3 - _DAT_0043d1b8) + DAT_0041c7b0;
      *piVar3 = iVar2;
      if (iVar4 <= iVar2) {
        iVar2 = iVar2 - _DAT_0043d1c4;
        (&DAT_0043ceb8)[iVar1] = 1;
        *piVar3 = iVar2;
      }
    }
    piVar3 = piVar3 + 1;
    iVar1 = iVar1 + 1;
  } while (piVar3 < &DAT_0043d5c8);
  iVar1 = (**(code **)(*DAT_0043d1bc + 0x28))(DAT_0043d1bc,0x10,local_200,&local_204);
  if (iVar1 == -0x7ff8ffe2) {
    DAT_0043d1c0 = 0;
    iVar1 = (**(code **)(*DAT_0043d1bc + 0x1c))(DAT_0043d1bc);
    if (-1 < iVar1) {
      DAT_0043d1c0 = 1;
      return;
    }
  }
  else {
    _DAT_0043d1b8 = DAT_0041c7b0;
  }
  return;
}



undefined1 __cdecl FUN_00404a90(byte param_1)

{
  return (&DAT_0043cdb0)[param_1];
}



void __cdecl FUN_00404aa0(undefined4 param_1,undefined4 param_2)

{
  if (DAT_0043cd64 != (code *)0x0) {
    (*DAT_0043cd64)(param_1,param_2);
  }
  return;
}



void __cdecl FUN_00404ac0(undefined4 param_1,int param_2)

{
  undefined1 uVar1;
  
  if (param_2 < 0x54) {
    uVar1 = (&DAT_0041c898)[param_2];
    *(undefined1 *)DAT_0043cd60 = uVar1;
    *(undefined1 *)(DAT_0043cd60 + 8) = uVar1;
    DAT_0043cd60 = (undefined4 *)((int)DAT_0043cd60 + 1);
    if (DAT_0043cd60 == (undefined4 *)&DAT_0043cd88) {
      DAT_0043cd60 = &DAT_0043cd68;
    }
  }
  return;
}



undefined4 FUN_00404b00(void)

{
  FUN_00404c30();
  FUN_00403570();
  FUN_00404b40();
  FUN_004046e0();
  FUN_00406310();
  FUN_004045e0(0);
  return 1;
}



undefined4 FUN_00404b30(void)

{
  FUN_00404b90();
  FUN_004038e0();
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00404b40(void)

{
  short sVar1;
  int iVar2;
  int iVar3;
  undefined4 *puVar4;
  
  if (DAT_0041c910 == 1) {
    return 1;
  }
  iVar2 = 0;
  DAT_0041c910 = 1;
  puVar4 = &DAT_0043e0c0;
  for (iVar3 = 200; iVar3 != 0; iVar3 = iVar3 + -1) {
    *puVar4 = 0;
    puVar4 = puVar4 + 1;
  }
  do {
    sVar1 = (short)iVar2;
    iVar2 = iVar2 + 1;
    *(short *)(iVar2 * 2 + 0x43dc0e) = sVar1 + 1;
  } while (iVar2 < 200);
  _DAT_0043ea20 = 0;
  return 1;
}



undefined4 FUN_00404b90(void)

{
  int iVar1;
  
  if (DAT_0041c910 == 0) {
    return 1;
  }
  iVar1 = 0;
  DAT_0041c910 = 0;
  do {
    if ((*(int *)((int)&DAT_0043e0c0 + iVar1) == 1) &&
       (*(int *)((int)&DAT_0043e700 + iVar1) == 0x10000)) {
      *(undefined4 *)((int)&DAT_0043e0c0 + iVar1) = 0;
      (**(code **)((int)&DAT_0043e3e0 + iVar1))(*(undefined4 *)((int)&DAT_0043d8f0 + iVar1));
    }
    iVar1 = iVar1 + 4;
  } while (iVar1 < 800);
  iVar1 = 0;
  do {
    if ((*(int *)((int)&DAT_0043e0c0 + iVar1) == 1) &&
       (*(int *)((int)&DAT_0043e700 + iVar1) == 0x20000)) {
      *(undefined4 *)((int)&DAT_0043e0c0 + iVar1) = 0;
      (**(code **)((int)&DAT_0043e3e0 + iVar1))(*(undefined4 *)((int)&DAT_0043d8f0 + iVar1));
    }
    iVar1 = iVar1 + 4;
  } while (iVar1 < 800);
  return 1;
}



undefined4 FUN_00404c30(void)

{
  _printf(s_Lisa_2_Development_System___s_0041c934,s_Compilation_0_91_0_0041c8f8);
  _printf(s_Copyright__c__UDS__1995_1996_0041c914);
  return 0;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00404c60(void)

{
  _DAT_0043cd10 = &LAB_00404d20;
  _DAT_0043cd14 = &LAB_00404d30;
  _DAT_0043cd18 = &LAB_00404d40;
  _DAT_0043cd1c = &LAB_00404d50;
  DAT_0043cd20 = FUN_00404d60;
  _DAT_0043cd24 = FUN_00404e20;
  _DAT_0043cd28 = FUN_00405050;
  _DAT_0043cd2c = &LAB_00404fb0;
  _DAT_0043cd30 = &LAB_00407000;
  _DAT_0043cd48 = FUN_00406860;
  _DAT_0043cd44 = &LAB_004070d0;
  _DAT_0043cd4c = &LAB_004070f0;
  _DAT_0043cd50 = &LAB_00407110;
  _DAT_0043cd54 = &LAB_00406f60;
  _DAT_0043cd5c = &LAB_00404f10;
  _DAT_0043cd58 = FUN_00404f70;
  FUN_00407fe0();
  FUN_00407190();
  FUN_00406fc0();
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 __cdecl
FUN_00404d60(undefined4 param_1,undefined4 param_2,int param_3,int param_4,undefined4 param_5)

{
  DAT_0041c958 = param_1;
  DAT_0043ea2c = param_2;
  DAT_0043eb64 = param_3;
  DAT_0043eb70 = param_4;
  DAT_0043eb6c = param_5;
  DAT_0043eb58 = 0;
  _DAT_0043ea28 = 0;
  DAT_0043eb5c = 0;
  _DAT_0043ea38 = 0;
  DAT_0043eb60 = param_3;
  _DAT_0043ea30 = param_3 << 8;
  DAT_0043eb68 = param_4;
  _DAT_0043ea3c = param_4 << 8;
  DAT_00453054 = 0;
  DAT_0045304c = 0;
  _DAT_00453050 = param_3;
  DAT_00453048 = param_4;
  DAT_00453060 = 0;
  DAT_00453044 = 0;
  DAT_00453064 = param_3 << 8;
  DAT_00453058 = param_4 << 8;
  DAT_00453068 = param_1;
  DAT_0045305c = param_2;
  _DAT_00453040 = param_4;
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 __cdecl FUN_00404e20(int param_1,int param_2,int param_3,int param_4)

{
  if ((((-1 < param_1) && (-1 < param_2)) && (param_1 < param_3)) &&
     (((param_2 < param_4 && (param_3 <= DAT_0043eb64)) && (param_4 <= DAT_0043eb70)))) {
    DAT_0043eb58 = param_1;
    _DAT_0043ea28 = param_1 << 8;
    DAT_0043eb5c = param_2;
    _DAT_0043ea38 = param_2 << 8;
    DAT_0043eb60 = param_3;
    _DAT_0043ea30 = param_3 << 8;
    DAT_0043eb68 = param_4;
    _DAT_0043ea3c = param_4 << 8;
    DAT_00453054 = param_1;
    DAT_0045304c = param_2;
    _DAT_00453050 = param_3;
    DAT_00453048 = param_4;
    DAT_00453060 = param_1 << 8;
    DAT_00453044 = param_2 << 8;
    DAT_00453064 = param_3 << 8;
    DAT_00453058 = param_4 << 8;
    return 1;
  }
  return 0;
}



undefined4 __cdecl FUN_00404f70(int *param_1)

{
  FUN_00404d60(param_1[9],param_1[7],*param_1,param_1[1],param_1[2]);
  FUN_00404e20(param_1[3],param_1[4],param_1[5],param_1[6]);
  return 1;
}



undefined4 __cdecl FUN_00405050(int *param_1)

{
  int iVar1;
  int *piVar2;
  
  iVar1 = *param_1;
  while (iVar1 != 0) {
    piVar2 = param_1 + 1;
    FUN_004050b0();
    iVar1 = *piVar2;
    while (iVar1 != 0) {
      DAT_0043ea34 = (int *)*piVar2;
      piVar2 = piVar2 + 1;
      iVar1 = *DAT_0043ea34;
      while (iVar1 != 0) {
        FUN_004050c0();
        DAT_0043ea34 = DAT_0043ea34 + DAT_0041c964;
        iVar1 = *DAT_0043ea34;
      }
      iVar1 = *piVar2;
    }
    param_1 = piVar2 + 1;
    iVar1 = *param_1;
  }
  return 1;
}



void FUN_004050b0(void)

{
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004050c0(void)

{
  int *piVar1;
  
  _DAT_0041c97c = *(undefined4 *)*DAT_0043ea34;
  _DAT_0041c980 = *(undefined4 *)(*DAT_0043ea34 + 4);
  _DAT_0041c984 = *(undefined4 *)DAT_0043ea34[1];
  _DAT_0041c988 = *(undefined4 *)(DAT_0043ea34[1] + 4);
  _DAT_0041c98c = *(undefined4 *)DAT_0043ea34[2];
  _DAT_0041c990 = *(undefined4 *)(DAT_0043ea34[2] + 4);
  piVar1 = DAT_0043ea34 + 3;
  _DAT_0041c994 = *(undefined4 *)(DAT_0043ea34[3] + 8);
  _DAT_0041c998 = *(undefined4 *)(*piVar1 + 0xc);
  _DAT_0041c99c = *(undefined4 *)(*piVar1 + 0x10);
  _DAT_0041c9a0 = *(undefined4 *)(*piVar1 + 0x14);
  _DAT_0041c9a4 = *(undefined4 *)(*piVar1 + 0x18);
  _DAT_0041c9a8 = *(undefined4 *)(*piVar1 + 0x1c);
  _DAT_0041c9ac = *(undefined4 *)*piVar1;
  FUN_00408040(0x41c978);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined8 FUN_00405170(void)

{
  int *piVar1;
  undefined8 uVar2;
  undefined4 local_40;
  int local_3c;
  int local_38;
  uint local_30;
  
  _DAT_0041c9dc = *DAT_0043ea34;
  _DAT_0041c9e4 = DAT_0043ea34[2];
  piVar1 = (int *)DAT_0043ea34[1];
  FUN_004067e0(&local_40,*piVar1);
  _DAT_0041c9b8 = piVar1[1];
  _DAT_0041c9bc = piVar1[2];
  _DAT_0041c9c0 = (local_30 & 0xff) * 0x100;
  _DAT_0041c9c4 = local_30 & 0xff00;
  _DAT_0041c9d0 = local_30 & 0xffff0000;
  _DAT_0041c9c8 = local_3c * 0x100 + _DAT_0041c9c0;
  _DAT_0041c9cc = local_38 * 0x100 + _DAT_0041c9c4;
  _DAT_0041c9d4 = DAT_0045306c;
  uVar2 = FUN_00408dc9(_DAT_0041c9d0,_DAT_0041c9bc);
  return uVar2;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00405220(void)

{
  _DAT_0043ccd8 = &LAB_004052c0;
  DAT_0043ccdc = &LAB_004052d0;
  _DAT_0043cce0 = FUN_00405900;
  DAT_0043cce4 = &LAB_00405bf0;
  _DAT_0043cce8 = &LAB_00405ce0;
  _DAT_0043ccec = &LAB_00405cf0;
  DAT_0043ccf0 = &LAB_00405d60;
  _DAT_0043ccf4 = &LAB_00405ef0;
  DAT_0043ccf8 = FUN_00405fd0;
  _DAT_0043ccfc = &LAB_00406040;
  _DAT_0043cd00 = FUN_00406050;
  _DAT_0043cd04 = FUN_004061a0;
  DAT_0043cd08 = &LAB_004061f0;
  _DAT_0043cd0c = FUN_00406250;
  return 1;
}



undefined4 FUN_00405900(void)

{
  int *piVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  int iVar5;
  int iVar6;
  undefined4 *puVar7;
  undefined4 *puVar8;
  int *piVar9;
  int *piStack_74;
  undefined4 uStack_70;
  undefined4 auStack_6c [5];
  int iStack_58;
  undefined4 uStack_4;
  
  piVar9 = (int *)&DAT_0043c914;
  do {
    if ((DAT_0041c79c != 0) && (piVar1 = (int *)*piVar9, piVar1 != (int *)0x0)) {
      (**(code **)(*piVar1 + 8))(piVar1);
      *piVar9 = 0;
      piVar9[-0xb] = 0;
    }
    piVar9 = piVar9 + 0xc;
  } while (piVar9 < &DAT_0043c944);
  piVar9 = &DAT_0043c944;
  do {
    piVar1 = (int *)*piVar9;
    if (piVar1 != (int *)0x0) {
      (**(code **)(*piVar1 + 8))(piVar1);
      *piVar9 = 0;
      piVar9[-0xb] = 0;
    }
    piVar9 = piVar9 + 0xc;
  } while (piVar9 < &DAT_0043cd04);
  if ((DAT_0041c79c != 0) && (DAT_0043ef78 != (int *)0x0)) {
    (**(code **)(*DAT_0043ef78 + 8))(DAT_0043ef78);
    DAT_0043ef78 = (int *)0x0;
  }
  FUN_00404530();
  if (DAT_0041c79c != 0) {
    iVar5 = (**(code **)(*DAT_0043f780 + 0x54))(DAT_0043f780,DAT_0041c870,DAT_0041c874,DAT_0041c878)
    ;
    if (iVar5 != 0) {
      return 0;
    }
  }
  puVar7 = auStack_6c;
  for (iVar5 = 0x1b; iVar5 != 0; iVar5 = iVar5 + -1) {
    *puVar7 = 0;
    puVar7 = puVar7 + 1;
  }
  auStack_6c[0] = 0x6c;
  if (DAT_0041c79c != 0) {
    iStack_58 = DAT_0041c87c;
    auStack_6c[1] = 0x21;
    uStack_4 = 0x218;
    iVar5 = (**(code **)(*DAT_0043f780 + 0x18))(DAT_0043f780,auStack_6c,&DAT_0043c914,0);
    if (iVar5 != 0) {
      return 0;
    }
    DAT_0043c8e8 = 1;
    DAT_0043c8ec = 1;
    DAT_0043c904 = DAT_0041c870;
    DAT_0043c908 = DAT_0041c874;
    DAT_0043c90c = DAT_0041c878;
    if (4 < DAT_0041c87c) {
      MessageBoxA(DAT_0043c7bc,s_The_maximum_amount_of_backbuffer_0041ca10,(LPCSTR)0x0,0);
      return 0;
    }
    if (0 < DAT_0041c87c) {
      iVar5 = 0;
      piStack_74 = DAT_0043c914;
      uStack_70 = 4;
      puVar7 = &DAT_0043c824;
      do {
        if (puVar7 != &DAT_0043c824) {
          uStack_70 = 0x10;
        }
        iVar6 = (**(code **)(*piStack_74 + 0x30))(piStack_74,&uStack_70,&piStack_74);
        uVar2 = DAT_0041c870;
        if (iVar6 != 0) {
          MessageBoxA(DAT_0043c7bc,s_Backbuffer_couldn_t_be_obtained_0041c9f0,(LPCSTR)0x0,0);
          return 0;
        }
        *puVar7 = piStack_74;
        uVar3 = DAT_0041c874;
        puVar7[-0xb] = 1;
        uVar4 = DAT_0041c878;
        puVar7[-10] = 1;
        iVar5 = iVar5 + 1;
        puVar7[-4] = uVar2;
        puVar7[-3] = uVar3;
        iVar6 = DAT_0041c87c;
        puVar7[-2] = uVar4;
        puVar7 = puVar7 + 0xc;
      } while (iVar5 < iVar6);
    }
  }
  auStack_6c[1] = 7;
  uStack_4 = 0x40;
  if (DAT_0041c79c != 0) {
    if (DAT_0043ef78 != (int *)0x0) {
      (**(code **)(*DAT_0043ef78 + 8))(DAT_0043ef78);
      DAT_0043ef78 = (int *)0x0;
    }
    DAT_0043f380 = 0;
    DAT_0043f381 = 0;
    DAT_0043f382 = 0;
    puVar7 = (undefined4 *)&DAT_0043f384;
    do {
      *(undefined1 *)puVar7 = 0xff;
      puVar8 = puVar7 + 1;
      *(undefined1 *)((int)puVar7 + 1) = 0xff;
      *(undefined1 *)((int)puVar7 + 2) = 0xff;
      puVar7 = puVar8;
    } while (puVar8 < &DAT_0043f780);
    iVar5 = (**(code **)(*DAT_0043f780 + 0x14))(DAT_0043f780,0x44,&DAT_0043f380,&DAT_0043ef78,0);
    if (iVar5 != 0) {
      return 0;
    }
    iVar5 = (**(code **)(*DAT_0043c914 + 0x7c))(DAT_0043c914,DAT_0043ef78);
    if (iVar5 != 0) {
      return 0;
    }
  }
  FUN_00406250();
  ShowWindow(DAT_0043c7bc,5);
  return 1;
}



// WARNING: Removing unreachable block (ram,0x00406006)

bool __cdecl FUN_00405fd0(int *param_1)

{
  int iVar1;
  
  if (param_1[10] != 1) {
    return false;
  }
  if (*param_1 != 0) {
    (**(code **)(*(int *)param_1[0xb] + 0x38))((int *)param_1[0xb]);
    do {
      iVar1 = (**(code **)(*DAT_0043c914 + 0x2c))(DAT_0043c914,param_1[0xb],0);
    } while (iVar1 == -0x7789fde4);
    return iVar1 == 0;
  }
  return false;
}



undefined4 __cdecl FUN_00406050(int *param_1,int param_2)

{
  int *piVar1;
  int iVar2;
  int unaff_ESI;
  int iStack_60;
  
  if (*param_1 == 0) {
    return 0;
  }
  if (param_1[2] == 1) {
    if (param_1[3] == param_2) {
      return 1;
    }
    piVar1 = (int *)param_1[0xb];
    iVar2 = (**(code **)(*piVar1 + 0x80))(piVar1,param_1 + 0xb);
    if (iVar2 != 0) {
      return 0;
    }
    param_1[3] = 0;
  }
  iVar2 = (**(code **)(*(int *)param_1[0xb] + 0x60))((int *)param_1[0xb]);
  if (iVar2 == -0x7789fe3e) {
    (**(code **)(*(int *)param_1[0xb] + 0x6c))((int *)param_1[0xb]);
    param_1[1] = 1;
  }
  if (param_2 == 1) {
    iVar2 = (**(code **)(*(int *)param_1[0xb] + 100))((int *)param_1[0xb],0,&stack0xffffff90,0x11,0)
    ;
    param_1[4] = iStack_60;
    param_1[5] = 0;
  }
  else {
    if (param_2 == 2) {
      iVar2 = (**(code **)(*(int *)param_1[0xb] + 100))((int *)param_1[0xb],0,&stack0xffffff90,1,0);
      param_1[4] = iStack_60;
    }
    else {
      if (param_2 != 3) {
        return 0;
      }
      iVar2 = (**(code **)(*(int *)param_1[0xb] + 100))
                        ((int *)param_1[0xb],0,&stack0xffffff90,0x21,0);
      param_1[4] = 0;
    }
    param_1[5] = iStack_60;
  }
  if (iVar2 != 0) {
    param_1[4] = 0;
    param_1[5] = 0;
    param_1[2] = 0;
    return 0;
  }
  param_1[3] = param_2;
  param_1[6] = unaff_ESI;
  param_1[2] = 1;
  return 1;
}



undefined4 __cdecl FUN_004061a0(int *param_1)

{
  int *piVar1;
  int iVar2;
  
  if (*param_1 == 0) {
    return 0;
  }
  if (param_1[2] == 0) {
    return 1;
  }
  piVar1 = (int *)param_1[0xb];
  iVar2 = (**(code **)(*piVar1 + 0x80))(piVar1,param_1 + 0xb);
  if (iVar2 != 0) {
    return 0;
  }
  param_1[2] = 0;
  param_1[3] = 0;
  return 1;
}



bool FUN_00406250(void)

{
  int iVar1;
  undefined4 *puVar2;
  int iVar3;
  
  iVar3 = 0;
  if ((DAT_0043c8e8 == 1) &&
     (iVar1 = (**(code **)(*DAT_0043c914 + 0x60))(DAT_0043c914), iVar1 == -0x7789fe3e)) {
    iVar3 = 1;
    (**(code **)(*DAT_0043c914 + 0x6c))(DAT_0043c914);
    DAT_0043c8ec = 1;
  }
  puVar2 = &DAT_0043c824;
  do {
    if ((puVar2[-0xb] == 1) &&
       (iVar1 = (**(code **)(*(int *)*puVar2 + 0x60))((int *)*puVar2), iVar1 == -0x7789fe3e)) {
      iVar3 = iVar3 + 1;
      (**(code **)(*(int *)*puVar2 + 0x6c))((int *)*puVar2);
      puVar2[-10] = 1;
    }
    puVar2 = puVar2 + 0xc;
  } while (puVar2 < &DAT_0043c914);
  puVar2 = &DAT_0043c944;
  do {
    if ((puVar2[-0xb] == 1) &&
       (iVar1 = (**(code **)(*(int *)*puVar2 + 0x60))((int *)*puVar2), iVar1 == -0x7789fe3e)) {
      iVar3 = iVar3 + 1;
      (**(code **)(*(int *)*puVar2 + 0x6c))((int *)*puVar2);
      puVar2[-10] = 1;
    }
    puVar2 = puVar2 + 0xc;
  } while (puVar2 < &DAT_0043cd04);
  return iVar3 < 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00406310(void)

{
  DAT_004449e8 = 0;
  DAT_004449ec = 0;
  DAT_004449f0 = 0;
  DAT_004449f4 = 0;
  DAT_00444b80 = 0;
  DAT_00444b84 = 0;
  DAT_00444b88 = 0;
  DAT_00444b8c = 0;
  _DAT_004450a0 = 0;
  DAT_004450a4 = 0;
  DAT_004450a8 = 0;
  DAT_004450ac = 0;
  _DAT_00445048 = 0;
  DAT_0044504c = 0;
  DAT_00445050 = 0;
  DAT_00445054 = 0;
  _DAT_00444b18 = 0;
  DAT_00444b1c = 0;
  DAT_00444b20 = 0;
  DAT_00444b24 = 0;
  _DAT_00444970 = 0;
  DAT_00444974 = 0;
  DAT_00444978 = 0;
  DAT_0044497c = 0;
  _DAT_00445088 = 0;
  DAT_0044508c = 0;
  DAT_00445090 = 0;
  DAT_00445094 = 0;
  DAT_00445068 = 0;
  _DAT_0044506c = 0;
  DAT_00445070 = 0;
  DAT_00445074 = 0;
  FUN_00406460();
  FUN_004065f0();
  FUN_004066d0();
  FUN_004069d0();
  FUN_00406630();
  FUN_00406520();
  FUN_00406590();
  FUN_00406640();
  return;
}



void FUN_00406460(void)

{
  FUN_00406f70((undefined4 *)&DAT_00444908);
  return;
}



void * __cdecl FUN_00406470(size_t param_1)

{
  void *pvVar1;
  
  if (param_1 == 0) {
    return (void *)0x0;
  }
  pvVar1 = _malloc(param_1);
  return pvVar1;
}



void __cdecl FUN_00406490(int param_1)

{
  undefined4 *_Memory;
  undefined4 uVar1;
  void *pvVar2;
  int iVar3;
  undefined4 *puVar4;
  int iVar5;
  
  _Memory = *(undefined4 **)(param_1 + 0xc);
  pvVar2 = _malloc(*(int *)(param_1 + 8) * 4 + 4000);
  iVar5 = 0;
  *(void **)(param_1 + 0xc) = pvVar2;
  if (0 < *(int *)(param_1 + 8)) {
    iVar3 = 0;
    puVar4 = _Memory;
    do {
      uVar1 = *puVar4;
      puVar4 = puVar4 + 1;
      iVar3 = iVar3 + 4;
      iVar5 = iVar5 + 1;
      *(undefined4 *)(*(int *)(param_1 + 0xc) + -4 + iVar3) = uVar1;
    } while (iVar5 < *(int *)(param_1 + 8));
  }
  iVar5 = *(int *)(param_1 + 8);
  if (iVar5 < iVar5 + 1000) {
    iVar3 = iVar5 * 4;
    do {
      iVar3 = iVar3 + 4;
      iVar5 = iVar5 + 1;
      *(undefined4 *)(*(int *)(param_1 + 0xc) + -4 + iVar3) = 0;
    } while (iVar5 < *(int *)(param_1 + 8) + 1000);
  }
  *(int *)(param_1 + 8) = *(int *)(param_1 + 8) + 1000;
  _free(_Memory);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00406520(void)

{
  _DAT_004448b0 = 0;
  _DAT_004448b8 = 0;
  _DAT_004448b4 = 0;
  _DAT_004448bc = 0;
  _DAT_004448c0 = 0;
  _DAT_004448c8 = 0;
  _DAT_004448c4 = 0;
  _DAT_004448cc = 0;
  _DAT_004448d0 = 0;
  _DAT_004448d4 = 0;
  _DAT_00445048 = 1;
  _DAT_004448d8 = 0;
  return;
}



void __cdecl FUN_00406570(void *param_1)

{
  if (param_1 != (void *)0x0) {
    _free(param_1);
  }
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00406590(void)

{
  _DAT_00444b18 = 8;
  return;
}



void __cdecl FUN_004065a0(char *param_1,undefined4 *param_2)

{
  char cVar1;
  void *pvVar2;
  int iVar3;
  int iVar4;
  size_t sVar5;
  
  iVar4 = 0;
  if (param_1 != (char *)0x0) {
    cVar1 = *param_1;
    while (cVar1 != '\0') {
      iVar4 = iVar4 + 1;
      cVar1 = param_1[iVar4];
    }
    sVar5 = iVar4 + 1;
    pvVar2 = FUN_00406470(sVar5);
    *param_2 = pvVar2;
    iVar4 = 0;
    if (0 < (int)sVar5) {
      do {
        iVar3 = iVar4 + 1;
        *(char *)(iVar4 + (int)pvVar2) = param_1[iVar4];
        iVar4 = iVar3;
      } while (iVar3 < (int)sVar5);
    }
    return;
  }
  *param_2 = 0;
  return;
}



void FUN_004065f0(void)

{
  int iVar1;
  int iVar2;
  longlong lVar3;
  
  iVar1 = 0;
  do {
    iVar2 = iVar1 + 1;
    fsin((float10)iVar1 * (float10)0.000244140625 * (float10)6.283192);
    lVar3 = __ftol();
    *(int *)(iVar2 * 4 + 0x440844) = (int)lVar3;
    iVar1 = iVar2;
  } while (iVar2 < 0x1000);
  return;
}



void FUN_00406630(void)

{
  DAT_00444b80 = 1;
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00406640(void)

{
  undefined4 *puVar1;
  
  FUN_00406f70((undefined4 *)&DAT_00445058);
  _DAT_0043f7c8 = s_default_0041ca4c;
  _DAT_0043f7d4 = 0x3f800000;
  _DAT_0043f7e4 = 0;
  _DAT_0043f7cc = 0;
  DAT_0043f7e8 = 0;
  _DAT_0043f7e0 = 0x3ecccccd;
  _DAT_0043f7dc = 0x3f000000;
  _DAT_0043f7d8 = 0x3dcccccd;
  puVar1 = (undefined4 *)FUN_00406470(0x10);
  _DAT_0043f7e9 = puVar1;
  *puVar1 = 0xc;
  puVar1[1] = 0;
  puVar1[2] = 0xfff;
  puVar1[3] = 0;
  _DAT_004450a0 = 1;
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004066d0(void)

{
  DAT_00444870 = s_default_0041ca4c;
  _DAT_00444874 = 0;
  _DAT_00444878 = 0;
  _DAT_0044487c = 0;
  _DAT_00444880 = 0;
  _DAT_00444884 = 0;
  _DAT_00444888 = 0;
  DAT_00445068 = 1;
  return;
}



void __cdecl FUN_00406710(undefined4 *param_1,int param_2)

{
  void *pvVar1;
  int iVar2;
  undefined4 *puVar3;
  undefined4 *puVar4;
  undefined4 *local_4;
  
  if ((param_2 < 0) || (DAT_00445070 <= param_2)) {
    param_2 = 0;
  }
  else {
    if (*(int *)(DAT_00445074 + param_2 * 4) == 0) {
      param_2 = 0;
    }
    local_4 = *(undefined4 **)(DAT_00445074 + param_2 * 4);
  }
  if (param_2 == 0) {
    local_4 = &DAT_00444870;
  }
  puVar3 = local_4;
  puVar4 = param_1;
  for (iVar2 = 0x10; iVar2 != 0; iVar2 = iVar2 + -1) {
    *puVar4 = *puVar3;
    puVar3 = puVar3 + 1;
    puVar4 = puVar4 + 1;
  }
  FUN_004065a0((char *)*local_4,param_1);
  param_1[6] = *param_1;
  pvVar1 = FUN_00406470(param_1[2] * param_1[1]);
  param_1[4] = pvVar1;
  param_1[5] = param_1[1];
  FUN_004067a0((int)local_4,(int)param_1);
  param_1[7] = param_1[4];
  return;
}



void __cdecl FUN_004067a0(int param_1,int param_2)

{
  int iVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  
  iVar3 = 0;
  iVar4 = *(int *)(param_1 + 0x10);
  iVar2 = *(int *)(param_2 + 0x10);
  if (0 < *(int *)(param_1 + 8)) {
    do {
      iVar1 = 0;
      if (0 < *(int *)(param_1 + 4)) {
        do {
          *(undefined1 *)(iVar1 + iVar2) = *(undefined1 *)(iVar1 + iVar4);
          iVar1 = iVar1 + 1;
        } while (iVar1 < *(int *)(param_1 + 4));
      }
      iVar4 = iVar4 + *(int *)(param_1 + 0x14);
      iVar2 = iVar2 + *(int *)(param_2 + 0x14);
      iVar3 = iVar3 + 1;
    } while (iVar3 < *(int *)(param_1 + 8));
  }
  return;
}



void __cdecl FUN_004067e0(undefined4 *param_1,int param_2)

{
  int iVar1;
  undefined4 *puVar2;
  undefined4 *puVar3;
  
  if (param_2 == 0) {
    puVar2 = &DAT_00444870;
    for (iVar1 = 0x10; iVar1 != 0; iVar1 = iVar1 + -1) {
      *param_1 = *puVar2;
      puVar2 = puVar2 + 1;
      param_1 = param_1 + 1;
    }
    return;
  }
  if ((-1 < param_2) && (param_2 < DAT_00445070)) {
    puVar2 = *(undefined4 **)(DAT_00445074 + param_2 * 4);
    if (puVar2 == (undefined4 *)0x0) {
      puVar2 = &DAT_00444870;
      for (iVar1 = 0x10; iVar1 != 0; iVar1 = iVar1 + -1) {
        *param_1 = *puVar2;
        puVar2 = puVar2 + 1;
        param_1 = param_1 + 1;
      }
      return;
    }
    puVar3 = param_1;
    for (iVar1 = 0x10; iVar1 != 0; iVar1 = iVar1 + -1) {
      *puVar3 = *puVar2;
      puVar2 = puVar2 + 1;
      puVar3 = puVar3 + 1;
    }
    param_1[6] = 0;
    param_1[7] = 0;
    return;
  }
  puVar2 = &DAT_00444870;
  for (iVar1 = 0x10; iVar1 != 0; iVar1 = iVar1 + -1) {
    *param_1 = *puVar2;
    puVar2 = puVar2 + 1;
    param_1 = param_1 + 1;
  }
  return;
}



int __cdecl FUN_00406860(undefined4 *param_1,int param_2)

{
  undefined4 *puVar1;
  int iVar2;
  void *pvVar3;
  int *piVar4;
  undefined4 *puVar5;
  
  while( true ) {
    if (param_1 == (undefined4 *)0x0) {
      if ((0 < param_2) && (param_2 < DAT_00445070)) {
        puVar1 = *(undefined4 **)(DAT_00445074 + param_2 * 4);
        if (puVar1 != (undefined4 *)0x0) {
          FUN_00406570((void *)*puVar1);
          FUN_00406d90(*(int *)(DAT_00445074 + param_2 * 4));
        }
        if (param_2 < DAT_00445068) {
          DAT_00445068 = param_2;
        }
        FUN_00406570(*(void **)(DAT_00445074 + param_2 * 4));
        *(undefined4 *)(DAT_00445074 + param_2 * 4) = 0;
      }
      return 0;
    }
    if (param_2 != 0) break;
    if (DAT_00445068 < DAT_00445070) {
      iVar2 = DAT_00445068 + 1;
      param_2 = DAT_00445068;
      if (DAT_00445070 <= iVar2) goto LAB_0040693b;
      piVar4 = (int *)(iVar2 * 4 + DAT_00445074);
      goto LAB_00406919;
    }
    FUN_00406490(0x445068);
  }
  if ((param_2 < 0) || (iVar2 = DAT_00445068, DAT_00445070 <= param_2)) {
    return 0;
  }
  goto LAB_0040693b;
  while( true ) {
    piVar4 = piVar4 + 1;
    iVar2 = iVar2 + 1;
    if (DAT_00445070 <= iVar2) break;
LAB_00406919:
    if (*piVar4 == 0) break;
  }
LAB_0040693b:
  DAT_00445068 = iVar2;
  piVar4 = (int *)(DAT_00445074 + param_2 * 4);
  puVar1 = (undefined4 *)*piVar4;
  if (puVar1 == (undefined4 *)0x0) {
    pvVar3 = FUN_00406470(0x40);
    *piVar4 = (int)pvVar3;
  }
  else {
    FUN_00406570((void *)*puVar1);
    FUN_00406d90(*(int *)(DAT_00445074 + param_2 * 4));
  }
  puVar1 = *(undefined4 **)(DAT_00445074 + param_2 * 4);
  puVar5 = puVar1;
  for (iVar2 = 0x10; iVar2 != 0; iVar2 = iVar2 + -1) {
    *puVar5 = *param_1;
    param_1 = param_1 + 1;
    puVar5 = puVar5 + 1;
  }
  FUN_004065a0((char *)*puVar1,puVar1);
  FUN_00406a10(puVar1);
  return param_2;
}



void __cdecl FUN_004069b0(int param_1)

{
  FUN_00406570(*(void **)(param_1 + 0x18));
  FUN_00406570(*(void **)(param_1 + 0x1c));
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004069d0(void)

{
  int iVar1;
  undefined4 *puVar2;
  
  _DAT_004448f8 = 0;
  _DAT_004448fc = 0;
  _DAT_004448f4 = 0;
  DAT_004448e8 = 0;
  _DAT_004448ec = 0;
  _DAT_004448f0 = 0;
  puVar2 = &DAT_00444c40;
  for (iVar1 = 0x101; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar2 = 0;
    puVar2 = puVar2 + 1;
  }
  DAT_00444b28 = 0;
  return;
}



void __cdecl FUN_00406a10(undefined4 *param_1)

{
  int *piVar1;
  int iVar2;
  int iVar3;
  int *piVar4;
  undefined4 *puVar5;
  int *piVar6;
  undefined4 *puVar7;
  int *local_48;
  undefined4 local_40 [16];
  
  if (0 < (int)param_1[2]) {
    while (((iVar3 = param_1[2], iVar3 < 0x101 && (0 < (int)param_1[1])) &&
           ((int)param_1[1] < 0x101))) {
      piVar4 = (int *)(&DAT_00444c40)[iVar3];
      if (piVar4 != (int *)0x0) {
        do {
          iVar3 = piVar4[1];
          piVar1 = FUN_00406d50(param_1[1],*(int **)(iVar3 + 0xc));
          if (piVar1 != (int *)0x0) {
            if (piVar1 == (int *)0xffffffff) {
              local_48 = (int *)FUN_00406470(0x18);
              piVar4 = &DAT_004448e8;
              piVar1 = local_48;
              for (iVar2 = 6; iVar2 != 0; iVar2 = iVar2 + -1) {
                *piVar1 = *piVar4;
                piVar4 = piVar4 + 1;
                piVar1 = piVar1 + 1;
              }
              local_48[1] = param_1[1];
              local_48[2] = *(int *)(iVar3 + 8) + *local_48;
              iVar2 = *(int *)(iVar3 + 0xc);
              local_48[5] = iVar2;
              if (iVar2 != 0) {
                *(int **)(iVar2 + 0x10) = local_48;
              }
              *(int **)(iVar3 + 0xc) = local_48;
            }
            else {
              local_48 = (int *)FUN_00406470(0x18);
              piVar4 = &DAT_004448e8;
              piVar6 = local_48;
              for (iVar2 = 6; iVar2 != 0; iVar2 = iVar2 + -1) {
                *piVar6 = *piVar4;
                piVar4 = piVar4 + 1;
                piVar6 = piVar6 + 1;
              }
              *local_48 = piVar1[1];
              local_48[1] = param_1[1] + piVar1[1];
              local_48[2] = *(int *)(iVar3 + 8) + *local_48;
              FUN_00406b70((int)piVar1,(int)local_48,piVar1[5]);
            }
            puVar5 = param_1;
            puVar7 = local_40;
            for (iVar3 = 0x10; iVar3 != 0; iVar3 = iVar3 + -1) {
              *puVar7 = *puVar5;
              puVar5 = puVar5 + 1;
              puVar7 = puVar7 + 1;
            }
            param_1[4] = local_48[2];
            param_1[5] = 0x100;
            FUN_004067a0((int)local_40,(int)param_1);
            return;
          }
          piVar4 = (int *)*piVar4;
        } while (piVar4 != (int *)0x0);
        iVar3 = param_1[2];
      }
      FUN_00406ba0(iVar3,DAT_00444b28);
      if ((int)param_1[2] < 1) {
        return;
      }
    }
  }
  return;
}



void __cdecl FUN_00406b70(int param_1,int param_2,int param_3)

{
  *(int *)(param_2 + 0x14) = param_3;
  *(int *)(param_2 + 0x10) = param_1;
  if (param_1 != 0) {
    *(int *)(param_1 + 0x14) = param_2;
  }
  if (param_3 != 0) {
    *(int *)(param_3 + 0x10) = param_2;
  }
  return;
}



void __cdecl FUN_00406ba0(int param_1,int param_2)

{
  int *piVar1;
  undefined4 *puVar2;
  int iVar3;
  int *piVar4;
  int *piVar5;
  int *local_4;
  
  do {
    while (param_2 == 0) {
      FUN_00406cc0();
      param_2 = DAT_00444b28;
    }
    do {
      piVar1 = FUN_00406d50(param_1,*(int **)(param_2 + 0xc));
      if (piVar1 != (int *)0x0) {
        if (piVar1 == (int *)0xffffffff) {
          local_4 = (int *)FUN_00406470(0x18);
          piVar1 = &DAT_004448e8;
          piVar4 = local_4;
          for (iVar3 = 6; iVar3 != 0; iVar3 = iVar3 + -1) {
            *piVar4 = *piVar1;
            piVar1 = piVar1 + 1;
            piVar4 = piVar4 + 1;
          }
          local_4[1] = param_1;
          local_4[2] = *(int *)(param_2 + 8) + *local_4 * 0x100;
          iVar3 = *(int *)(param_2 + 0xc);
          local_4[5] = iVar3;
          if (iVar3 != 0) {
            *(int **)(iVar3 + 0x10) = local_4;
          }
          *(int **)(param_2 + 0xc) = local_4;
        }
        else {
          local_4 = (int *)FUN_00406470(0x18);
          piVar4 = &DAT_004448e8;
          piVar5 = local_4;
          for (iVar3 = 6; iVar3 != 0; iVar3 = iVar3 + -1) {
            *piVar5 = *piVar4;
            piVar4 = piVar4 + 1;
            piVar5 = piVar5 + 1;
          }
          *local_4 = piVar1[1];
          local_4[1] = piVar1[1] + param_1;
          local_4[2] = *(int *)(param_2 + 8) + *local_4 * 0x100;
          FUN_00406b70((int)piVar1,(int)local_4,piVar1[5]);
        }
        puVar2 = (undefined4 *)FUN_00406470(8);
        *puVar2 = (&DAT_00444c40)[param_1];
        puVar2[1] = local_4;
        (&DAT_00444c40)[param_1] = puVar2;
        return;
      }
      param_2 = *(int *)(param_2 + 0x14);
    } while (param_2 != 0);
    FUN_00406cc0();
    param_2 = DAT_00444b28;
  } while( true );
}



void FUN_00406cc0(void)

{
  undefined4 *puVar1;
  int iVar2;
  undefined4 *puVar3;
  undefined4 *puVar4;
  
  puVar1 = (undefined4 *)FUN_00406470(0x18);
  puVar3 = &DAT_004448e8;
  puVar4 = puVar1;
  for (iVar2 = 6; iVar2 != 0; iVar2 = iVar2 + -1) {
    *puVar4 = *puVar3;
    puVar3 = puVar3 + 1;
    puVar4 = puVar4 + 1;
  }
  puVar1[5] = DAT_00444b28;
  iVar2 = FUN_00406d20(0x10000,0x10000);
  puVar1[2] = iVar2;
  if (DAT_00444b28 != (undefined4 *)0x0) {
    *(undefined4 **)((int)DAT_00444b28 + 0x10) = puVar1;
  }
  DAT_00444b28 = puVar1;
  return;
}



int __cdecl FUN_00406d20(int param_1,uint param_2)

{
  void *pvVar1;
  uint uVar2;
  
  pvVar1 = FUN_00406470(param_1 + param_2 + 4);
  uVar2 = ((int)pvVar1 + 4U) % param_2;
  *(void **)((int)pvVar1 + (param_2 - uVar2)) = pvVar1;
  return (int)pvVar1 + param_2 + (4 - uVar2);
}



int * __cdecl FUN_00406d50(int param_1,int *param_2)

{
  int *piVar1;
  int iVar2;
  
  if (param_2 == (int *)0x0) {
    return (int *)0xffffffff;
  }
  if (*param_2 < param_1) {
    while( true ) {
      piVar1 = (int *)param_2[5];
      iVar2 = 0x100;
      if (piVar1 != (int *)0x0) {
        iVar2 = *piVar1;
      }
      if (param_1 <= iVar2 - param_2[1]) break;
      param_2 = piVar1;
      if (piVar1 == (int *)0x0) {
        return (int *)0x0;
      }
    }
    return param_2;
  }
  return (int *)0xffffffff;
}



void __cdecl FUN_00406d90(int param_1)

{
  int *piVar1;
  int iVar2;
  uint uVar3;
  void *pvVar4;
  int *piVar5;
  void *pvVar6;
  void *pvVar7;
  int *local_4;
  
  iVar2 = *(int *)(param_1 + 8);
  if ((((0 < iVar2) && (iVar2 < 0x101)) && (0 < *(int *)(param_1 + 4))) &&
     (*(int *)(param_1 + 4) < 0x101)) {
    uVar3 = *(uint *)(param_1 + 0x10);
    pvVar7 = DAT_00444b28;
    if (DAT_00444b28 != (void *)0x0) {
      while (*(uint *)((int)pvVar7 + 8) != (uVar3 & 0xffff0000)) {
        pvVar7 = *(void **)((int)pvVar7 + 0x14);
        if (pvVar7 == (void *)0x0) {
          return;
        }
      }
      pvVar4 = *(void **)((int)pvVar7 + 0xc);
      while (*(uint *)((int)pvVar4 + 8) != (uVar3 & 0xffffff00)) {
        pvVar4 = *(void **)((int)pvVar4 + 0x14);
        if (pvVar4 == (void *)0x0) {
          return;
        }
      }
      pvVar6 = *(void **)((int)pvVar4 + 0xc);
      while (*(uint *)((int)pvVar6 + 8) != uVar3) {
        pvVar6 = *(void **)((int)pvVar6 + 0x14);
        if (pvVar6 == (void *)0x0) {
          return;
        }
      }
      local_4 = (int *)0x0;
      piVar1 = (int *)(&DAT_00444c40)[iVar2];
      while (piVar5 = piVar1, (void *)piVar5[1] != pvVar4) {
        piVar1 = (int *)*piVar5;
        local_4 = piVar5;
        if ((int *)*piVar5 == (int *)0x0) {
          return;
        }
      }
      iVar2 = *(int *)((int)pvVar6 + 0x14);
      if ((iVar2 == 0) && (*(int *)((int)pvVar6 + 0x10) == 0)) {
        iVar2 = *(int *)((int)pvVar4 + 0x14);
        if ((iVar2 == 0) && (*(int *)((int)pvVar4 + 0x10) == 0)) {
          FUN_00406f40(*(int *)((int)pvVar7 + 8));
          piVar1 = (int *)((int)pvVar7 + 0x14);
          if (*(int *)((int)pvVar7 + 0x10) == 0) {
            DAT_00444b28 = (void *)*piVar1;
          }
          else {
            *(int *)(*(int *)((int)pvVar7 + 0x10) + 0x14) = *piVar1;
          }
          if (*piVar1 != 0) {
            *(undefined4 *)(*piVar1 + 0x10) = *(undefined4 *)((int)pvVar7 + 0x10);
          }
          FUN_00406570(pvVar7);
        }
        else {
          if (*(int *)((int)pvVar4 + 0x10) == 0) {
            *(int *)((int)pvVar7 + 0xc) = iVar2;
          }
          else {
            *(int *)(*(int *)((int)pvVar4 + 0x10) + 0x14) = iVar2;
          }
          if (*(int *)((int)pvVar4 + 0x14) != 0) {
            *(undefined4 *)(*(int *)((int)pvVar4 + 0x14) + 0x10) =
                 *(undefined4 *)((int)pvVar4 + 0x10);
          }
        }
        if (local_4 == (int *)0x0) {
          (&DAT_00444c40)[*(int *)(param_1 + 8)] = *piVar5;
        }
        else {
          *local_4 = *piVar5;
        }
        FUN_00406570(piVar5);
        FUN_00406570(pvVar4);
      }
      else {
        if (*(int *)((int)pvVar6 + 0x10) == 0) {
          *(int *)((int)pvVar4 + 0xc) = iVar2;
        }
        else {
          *(int *)(*(int *)((int)pvVar6 + 0x10) + 0x14) = iVar2;
        }
        if (*(int *)((int)pvVar6 + 0x14) != 0) {
          *(undefined4 *)(*(int *)((int)pvVar6 + 0x14) + 0x10) = *(undefined4 *)((int)pvVar6 + 0x10)
          ;
        }
      }
      FUN_00406570(pvVar6);
    }
  }
  return;
}



void __cdecl FUN_00406f40(int param_1)

{
  FUN_00406570(*(void **)(param_1 + -4));
  return;
}



void __cdecl FUN_00406f70(undefined4 *param_1)

{
  void *pvVar1;
  int iVar2;
  
  pvVar1 = FUN_00406470(0x80);
  iVar2 = 0;
  param_1[3] = pvVar1;
  do {
    iVar2 = iVar2 + 8;
    *(undefined4 *)(param_1[3] + -8 + iVar2) = 0;
    *(undefined4 *)(param_1[3] + -4 + iVar2) = 0;
  } while (iVar2 < 0x80);
  param_1[2] = 0xf;
  param_1[1] = 0;
  *param_1 = 0x10;
  return;
}



undefined4 FUN_00406fc0(void)

{
  undefined4 *puVar1;
  undefined4 *puVar2;
  
  puVar2 = &DAT_00447038;
  puVar1 = &DAT_004450b8;
  DAT_0044cdf8 = &DAT_00446ff8;
  do {
    *puVar1 = puVar2;
    puVar1 = puVar1 + 1;
    *puVar2 = 0;
    puVar2 = puVar2 + 3;
  } while (puVar1 < &DAT_00446ff8);
  FUN_004067e0((undefined4 *)&DAT_00446ff8,0);
  return 1;
}



void __cdecl
FUN_00407130(undefined4 param_1,undefined4 param_2,undefined4 param_3,undefined4 param_4)

{
  FUN_00408fa4(param_3,param_4);
  return;
}



void __cdecl FUN_00407150(undefined4 param_1,undefined4 param_2,uint param_3)

{
  FUN_00408fca(param_3);
  return;
}



void __cdecl FUN_00407170(undefined4 param_1,undefined4 param_2,uint param_3)

{
  FUN_0040901f(param_3);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00407190(void)

{
  _DAT_0044ce2c = 0;
  if (DAT_0041ca54 == 0) {
    DAT_0044ce38 = _malloc(0x20d8);
    DAT_0044ce3c = _malloc(0x20d8);
    DAT_0044ce40 = _malloc(0x20d8);
    DAT_0041ca54 = 1;
  }
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void __cdecl FUN_004071f0(int param_1)

{
  int iVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  
  iVar4 = *(int *)(param_1 + 4);
  iVar5 = *(int *)(param_1 + 8);
  iVar7 = *(int *)(param_1 + 0xc);
  iVar1 = *(int *)(param_1 + 0x10);
  DAT_0041cabc = *(int *)(param_1 + 0x14);
  DAT_0041cac0 = *(int *)(param_1 + 0x18);
  uVar2 = *(undefined4 *)(param_1 + 0x24);
  uVar3 = *(undefined4 *)(param_1 + 0x28);
  DAT_0044ce24 = *(undefined4 *)(param_1 + 0x34);
  DAT_0044ce30 = DAT_00453068;
  DAT_0044ce34 = iVar4;
  DAT_0044ce58 = iVar7;
  if (iVar7 < iVar4) {
    DAT_0044ce34 = iVar7;
    DAT_0044ce58 = iVar4;
  }
  iVar6 = DAT_0041cabc;
  if ((DAT_0041cabc <= DAT_0044ce58) && (iVar6 = DAT_0044ce58, DAT_0041cabc < DAT_0044ce34)) {
    DAT_0044ce34 = DAT_0041cabc;
  }
  DAT_0044ce58 = iVar6;
  DAT_0041caac = iVar4;
  DAT_0041cab0 = iVar5;
  DAT_0041cac4 = *(undefined4 *)(param_1 + 0x1c);
  DAT_0041cac8 = *(undefined4 *)(param_1 + 0x20);
  DAT_0041cad4 = *(undefined4 *)(param_1 + 0x2c);
  DAT_0041cad8 = *(undefined4 *)(param_1 + 0x30);
  if (DAT_0041cac0 < iVar5) {
    DAT_0041caac = DAT_0041cabc;
    DAT_0041cab0 = DAT_0041cac0;
    DAT_0041cabc = iVar4;
    DAT_0041cac0 = iVar5;
    DAT_0041cac4 = *(undefined4 *)(param_1 + 0x2c);
    DAT_0041cac8 = *(undefined4 *)(param_1 + 0x30);
    DAT_0041cad4 = *(undefined4 *)(param_1 + 0x1c);
    DAT_0041cad8 = *(undefined4 *)(param_1 + 0x20);
  }
  if (iVar1 < DAT_0041cab0) {
    DAT_0041cab8 = DAT_0041cab0;
    DAT_0041cab4 = DAT_0041caac;
    DAT_0041cad0 = DAT_0041cac8;
    DAT_0041cacc = DAT_0041cac4;
    DAT_0041caac = iVar7;
    DAT_0041cab0 = iVar1;
    DAT_0041cac4 = uVar2;
    DAT_0041cac8 = uVar3;
  }
  else {
    DAT_0041cab4 = iVar7;
    DAT_0041cab8 = iVar1;
    DAT_0041cacc = uVar2;
    DAT_0041cad0 = uVar3;
    if (DAT_0041cac0 < iVar1) {
      DAT_0041cab8 = DAT_0041cac0;
      DAT_0041cab4 = DAT_0041cabc;
      DAT_0041cad0 = DAT_0041cad8;
      DAT_0041cacc = DAT_0041cad4;
      DAT_0041cabc = iVar7;
      DAT_0041cac0 = iVar1;
      DAT_0041cad4 = uVar2;
      DAT_0041cad8 = uVar3;
    }
  }
  if (DAT_0041cab0 < DAT_00453044) {
    FUN_00407910(param_1);
    return;
  }
  if (DAT_00453058 < DAT_0041cac0) {
    FUN_00407910(param_1);
    return;
  }
  if (DAT_0044ce58 <= DAT_00453064) {
    if (DAT_0044ce34 < DAT_00453060) {
      FUN_00407910(param_1);
      return;
    }
    FUN_00456000();
    FUN_00407130(DAT_0041cae0,DAT_0041cadc,DAT_0044ce24,*(undefined4 *)(param_1 + 0x38));
    iVar4 = FUN_00407f40(DAT_0041caac,DAT_0041cab0,DAT_0041cabc,DAT_0041cac0);
    iVar5 = FUN_00407f40(DAT_0041caac,DAT_0041cab0,DAT_0041cab4,DAT_0041cab8);
    _DAT_0044ce04 = DAT_0041cab0;
    _DAT_0044ce00 = DAT_0041caac;
    _DAT_0044ce08 = DAT_0041cab4;
    _DAT_0044ce0c = DAT_0041cab8;
    _DAT_0044ce10 = DAT_0041cac4;
    _DAT_0044ce14 = DAT_0041cac8;
    _DAT_0044ce18 = DAT_0041cacc;
    _DAT_0044ce1c = DAT_0041cad0;
    if (iVar5 < iVar4) {
      FUN_004561c0();
      _DAT_0044ce00 = DAT_0041caac;
      _DAT_0044ce04 = DAT_0041cab0;
      _DAT_0044ce08 = DAT_0041cabc;
      _DAT_0044ce0c = DAT_0041cac0;
      _DAT_0044ce10 = DAT_0041cac4;
      _DAT_0044ce14 = DAT_0041cac8;
      _DAT_0044ce18 = DAT_0041cad4;
      _DAT_0044ce1c = DAT_0041cad8;
      FUN_00456180();
      _DAT_0044ce00 = DAT_0041cab4;
      _DAT_0044ce04 = DAT_0041cab8;
      _DAT_0044ce08 = DAT_0041cabc;
      _DAT_0044ce0c = DAT_0041cac0;
      _DAT_0044ce10 = DAT_0041cacc;
      _DAT_0044ce14 = DAT_0041cad0;
      _DAT_0044ce18 = DAT_0041cad4;
      _DAT_0044ce1c = DAT_0041cad8;
      FUN_004561c0();
      iVar4 = 0;
      DAT_0044ce30 = DAT_0044ce30 + *DAT_0044ce38 * DAT_0045305c;
      if (0 < DAT_0044ce38[1]) {
        iVar5 = 0;
        do {
          iVar7 = *(int *)((int)DAT_0044ce38 + iVar5 + 0x10);
          iVar4 = iVar4 + 1;
          FUN_00407150(DAT_0044ce30 + iVar7 + 1,
                       (*(int *)((int)DAT_0044ce3c + iVar5 + 0x10) - iVar7) + -1,
                       (*(uint *)((int)DAT_0044ce3c + iVar5 + 8) & 0xffff) +
                       *(int *)((int)DAT_0044ce3c + iVar5 + 0xc) * 0x1000000);
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
        } while (iVar4 < DAT_0044ce38[1]);
      }
      iVar5 = 0;
      if (0 < *(int *)(DAT_0044ce40 + 4)) {
        iVar7 = 0;
        iVar4 = iVar4 * 0xc;
        do {
          iVar1 = *(int *)(DAT_0044ce40 + iVar7 + 0x10);
          iVar7 = iVar7 + 0xc;
          iVar5 = iVar5 + 1;
          FUN_00407150(DAT_0044ce30 + iVar1 + 1,
                       (*(int *)((int)DAT_0044ce3c + iVar4 + 0x10) - iVar1) + -1,
                       (*(uint *)((int)DAT_0044ce3c + iVar4 + 8) & 0xffff) +
                       *(int *)((int)DAT_0044ce3c + iVar4 + 0xc) * 0x1000000);
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
          iVar4 = iVar4 + 0xc;
        } while (iVar5 < *(int *)(DAT_0044ce40 + 4));
        return;
      }
    }
    else {
      FUN_00456180();
      _DAT_0044ce00 = DAT_0041caac;
      _DAT_0044ce04 = DAT_0041cab0;
      _DAT_0044ce08 = DAT_0041cabc;
      _DAT_0044ce0c = DAT_0041cac0;
      _DAT_0044ce10 = DAT_0041cac4;
      _DAT_0044ce14 = DAT_0041cac8;
      _DAT_0044ce18 = DAT_0041cad4;
      _DAT_0044ce1c = DAT_0041cad8;
      FUN_004561c0();
      _DAT_0044ce00 = DAT_0041cab4;
      _DAT_0044ce04 = DAT_0041cab8;
      _DAT_0044ce08 = DAT_0041cabc;
      _DAT_0044ce0c = DAT_0041cac0;
      _DAT_0044ce10 = DAT_0041cacc;
      _DAT_0044ce14 = DAT_0041cad0;
      _DAT_0044ce18 = DAT_0041cad4;
      _DAT_0044ce1c = DAT_0041cad8;
      FUN_00456180();
      iVar5 = 0;
      DAT_0044ce30 = DAT_0044ce30 + *DAT_0044ce3c * DAT_0045305c;
      iVar4 = 0;
      if (0 < DAT_0044ce38[1]) {
        do {
          iVar7 = *(int *)((int)DAT_0044ce3c + iVar4 + 0x10);
          iVar5 = iVar5 + 1;
          FUN_00407150(DAT_0044ce30 + iVar7 + 1,
                       (*(int *)((int)DAT_0044ce38 + iVar4 + 0x10) - iVar7) + -1,
                       (*(uint *)((int)DAT_0044ce38 + iVar4 + 8) & 0xffff) +
                       *(int *)((int)DAT_0044ce38 + iVar4 + 0xc) * 0x1000000);
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
          iVar4 = iVar4 + 0xc;
        } while (iVar5 < DAT_0044ce38[1]);
      }
      iVar7 = 0;
      iVar4 = 0;
      if (0 < *(int *)(DAT_0044ce40 + 4)) {
        iVar5 = iVar5 * 0xc;
        do {
          iVar6 = DAT_0044ce40 + iVar7;
          iVar1 = *(int *)((int)DAT_0044ce3c + iVar5 + 0x10);
          iVar7 = iVar7 + 0xc;
          iVar4 = iVar4 + 1;
          FUN_00407150(DAT_0044ce30 + iVar1 + 1,(*(int *)(iVar6 + 0x10) - iVar1) + -1,
                       (*(uint *)(iVar6 + 8) & 0xffff) + *(int *)(iVar6 + 0xc) * 0x1000000);
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
        } while (iVar4 < *(int *)(DAT_0044ce40 + 4));
      }
    }
    return;
  }
  FUN_00407910(param_1);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void __cdecl FUN_00407910(int param_1)

{
  int iVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  
  if ((((DAT_00453044 <= DAT_0041cac0) && (DAT_0041cab0 <= DAT_00453058)) &&
      (DAT_0044ce34 <= DAT_00453064)) && (DAT_00453060 <= DAT_0044ce58)) {
    FUN_00456000();
    FUN_00407130(DAT_0041cae0,DAT_0041cadc,DAT_0044ce24,*(undefined4 *)(param_1 + 0x38));
    iVar1 = FUN_00407f40(DAT_0041cab4,DAT_0041cab8,DAT_0041cabc,DAT_0041cac0);
    iVar2 = FUN_00407f40(DAT_0041caac,DAT_0041cab0,DAT_0041cab4,DAT_0041cab8);
    _DAT_0044ce64 = DAT_0041cab0;
    _DAT_0044ce60 = DAT_0041caac;
    _DAT_0044ce68 = DAT_0041cab4;
    _DAT_0044ce6c = DAT_0041cab8;
    _DAT_0044ce70 = DAT_0041cac4;
    _DAT_0044ce74 = DAT_0041cac8;
    _DAT_0044ce78 = DAT_0041cacc;
    _DAT_0044ce7c = DAT_0041cad0;
    if (iVar2 < iVar1) {
      FUN_00456640();
      _DAT_0044ce60 = DAT_0041caac;
      _DAT_0044ce64 = DAT_0041cab0;
      _DAT_0044ce68 = DAT_0041cabc;
      _DAT_0044ce6c = DAT_0041cac0;
      _DAT_0044ce70 = DAT_0041cac4;
      _DAT_0044ce74 = DAT_0041cac8;
      _DAT_0044ce78 = DAT_0041cad4;
      _DAT_0044ce7c = DAT_0041cad8;
      FUN_00456520();
      _DAT_0044ce60 = DAT_0041cab4;
      _DAT_0044ce64 = DAT_0041cab8;
      _DAT_0044ce68 = DAT_0041cabc;
      _DAT_0044ce6c = DAT_0041cac0;
      _DAT_0044ce70 = DAT_0041cacc;
      _DAT_0044ce74 = DAT_0041cad0;
      _DAT_0044ce78 = DAT_0041cad4;
      _DAT_0044ce7c = DAT_0041cad8;
      FUN_00456640();
      iVar1 = 0;
      DAT_0044ce30 = DAT_0044ce30 + *DAT_0044ce38 * DAT_0045305c;
      if (0 < DAT_0044ce38[1]) {
        iVar2 = 0;
        do {
          iVar5 = *(int *)((int)DAT_0044ce38 + iVar2 + 0x10);
          if (iVar5 < DAT_00453054) {
            FUN_00407170(DAT_00453054 + DAT_0044ce30,
                         *(int *)((int)DAT_0044ce3c + iVar2 + 0x10) - DAT_00453054,
                         (*(uint *)((int)DAT_0044ce3c + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044ce3c + iVar2 + 0xc) * 0x1000000);
          }
          else {
            FUN_00407150(DAT_0044ce30 + iVar5 + 1,
                         (*(int *)((int)DAT_0044ce3c + iVar2 + 0x10) - iVar5) + -1,
                         (*(uint *)((int)DAT_0044ce3c + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044ce3c + iVar2 + 0xc) * 0x1000000);
          }
          iVar2 = iVar2 + 0xc;
          iVar1 = iVar1 + 1;
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
        } while (iVar1 < DAT_0044ce38[1]);
      }
      iVar2 = 0;
      if (0 < *(int *)(DAT_0044ce40 + 4)) {
        iVar5 = 0;
        iVar1 = iVar1 * 0xc;
        do {
          iVar3 = *(int *)(DAT_0044ce40 + iVar5 + 0x10);
          if (iVar3 < DAT_00453054) {
            FUN_00407170(DAT_00453054 + DAT_0044ce30,
                         *(int *)((int)DAT_0044ce3c + iVar1 + 0x10) - DAT_00453054,
                         (*(uint *)((int)DAT_0044ce3c + iVar1 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044ce3c + iVar1 + 0xc) * 0x1000000);
          }
          else {
            FUN_00407150(DAT_0044ce30 + iVar3 + 1,
                         (*(int *)((int)DAT_0044ce3c + iVar1 + 0x10) - iVar3) + -1,
                         (*(uint *)((int)DAT_0044ce3c + iVar1 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044ce3c + iVar1 + 0xc) * 0x1000000);
          }
          iVar1 = iVar1 + 0xc;
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
          iVar2 = iVar2 + 1;
        } while (iVar2 < *(int *)(DAT_0044ce40 + 4));
        return;
      }
    }
    else {
      FUN_00456520();
      _DAT_0044ce60 = DAT_0041caac;
      _DAT_0044ce64 = DAT_0041cab0;
      _DAT_0044ce68 = DAT_0041cabc;
      _DAT_0044ce6c = DAT_0041cac0;
      _DAT_0044ce70 = DAT_0041cac4;
      _DAT_0044ce74 = DAT_0041cac8;
      _DAT_0044ce78 = DAT_0041cad4;
      _DAT_0044ce7c = DAT_0041cad8;
      FUN_00456640();
      _DAT_0044ce60 = DAT_0041cab4;
      _DAT_0044ce64 = DAT_0041cab8;
      _DAT_0044ce68 = DAT_0041cabc;
      _DAT_0044ce6c = DAT_0041cac0;
      _DAT_0044ce70 = DAT_0041cacc;
      _DAT_0044ce74 = DAT_0041cad0;
      _DAT_0044ce78 = DAT_0041cad4;
      _DAT_0044ce7c = DAT_0041cad8;
      FUN_00456520();
      iVar2 = 0;
      iVar1 = 0;
      DAT_0044ce30 = DAT_0044ce30 + *DAT_0044ce3c * DAT_0045305c;
      if (0 < DAT_0044ce38[1]) {
        do {
          iVar5 = *(int *)((int)DAT_0044ce3c + iVar2 + 0x10);
          if (iVar5 < DAT_00453054) {
            FUN_00407170(DAT_00453054 + DAT_0044ce30,
                         *(int *)((int)DAT_0044ce38 + iVar2 + 0x10) - DAT_00453054,
                         (*(uint *)((int)DAT_0044ce38 + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044ce38 + iVar2 + 0xc) * 0x1000000);
          }
          else {
            FUN_00407150(DAT_0044ce30 + iVar5 + 1,
                         (*(int *)((int)DAT_0044ce38 + iVar2 + 0x10) - iVar5) + -1,
                         (*(uint *)((int)DAT_0044ce38 + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044ce38 + iVar2 + 0xc) * 0x1000000);
          }
          iVar2 = iVar2 + 0xc;
          iVar1 = iVar1 + 1;
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
        } while (iVar1 < DAT_0044ce38[1]);
      }
      iVar5 = 0;
      iVar2 = 0;
      if (0 < *(int *)(DAT_0044ce40 + 4)) {
        iVar1 = iVar1 * 0xc;
        do {
          iVar3 = *(int *)((int)DAT_0044ce3c + iVar1 + 0x10);
          if (iVar3 < DAT_00453054) {
            iVar3 = DAT_0044ce40 + iVar5;
            FUN_00407170(DAT_00453054 + DAT_0044ce30,*(int *)(iVar3 + 0x10) - DAT_00453054,
                         (*(uint *)(iVar3 + 8) & 0xffff) + *(int *)(iVar3 + 0xc) * 0x1000000);
          }
          else {
            iVar4 = DAT_0044ce40 + iVar5;
            FUN_00407150(DAT_0044ce30 + iVar3 + 1,(*(int *)(iVar4 + 0x10) - iVar3) + -1,
                         (*(uint *)(iVar4 + 8) & 0xffff) + *(int *)(iVar4 + 0xc) * 0x1000000);
          }
          iVar1 = iVar1 + 0xc;
          DAT_0044ce30 = DAT_0044ce30 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
          iVar2 = iVar2 + 1;
        } while (iVar2 < *(int *)(DAT_0044ce40 + 4));
      }
    }
  }
  return;
}



int __cdecl FUN_00407f40(int param_1,int param_2,int param_3,int param_4)

{
  int iVar1;
  
  if (param_4 == param_2) {
    iVar1 = -0x7fff0000;
    if (param_1 <= param_3) {
      return 0x7fff0000;
    }
  }
  else {
    iVar1 = ((param_1 - param_3) * 0x100) / (param_2 - param_4);
  }
  return iVar1;
}



void __cdecl FUN_00407f80(undefined4 param_1,undefined4 param_2,undefined4 param_3)

{
  FUN_004090b8(param_3);
  return;
}



void __cdecl FUN_00407fa0(undefined4 param_1,undefined4 param_2,uint param_3)

{
  FUN_004090d8(param_3);
  return;
}



void __cdecl FUN_00407fc0(undefined4 param_1,undefined4 param_2,uint param_3)

{
  FUN_0040914e(param_3);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00407fe0(void)

{
  _DAT_0044ceb0 = 0;
  if (DAT_0041ca58 == 0) {
    DAT_0044cebc = _malloc(0x20d8);
    DAT_0044cec0 = _malloc(0x20d8);
    DAT_0044cec4 = _malloc(0x20d8);
    DAT_0041ca58 = 1;
  }
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void __cdecl FUN_00408040(int param_1)

{
  int iVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  
  iVar4 = *(int *)(param_1 + 4);
  iVar5 = *(int *)(param_1 + 8);
  iVar7 = *(int *)(param_1 + 0xc);
  iVar1 = *(int *)(param_1 + 0x10);
  DAT_0041cabc = *(int *)(param_1 + 0x14);
  DAT_0041cac0 = *(int *)(param_1 + 0x18);
  uVar2 = *(undefined4 *)(param_1 + 0x24);
  uVar3 = *(undefined4 *)(param_1 + 0x28);
  DAT_0044cea8 = *(undefined4 *)(param_1 + 0x34);
  DAT_0044ceb4 = DAT_00453068;
  DAT_0044ceb8 = iVar4;
  DAT_0044ced8 = iVar7;
  if (iVar7 < iVar4) {
    DAT_0044ceb8 = iVar7;
    DAT_0044ced8 = iVar4;
  }
  iVar6 = DAT_0041cabc;
  if ((DAT_0041cabc <= DAT_0044ced8) && (iVar6 = DAT_0044ced8, DAT_0041cabc < DAT_0044ceb8)) {
    DAT_0044ceb8 = DAT_0041cabc;
  }
  DAT_0044ced8 = iVar6;
  DAT_0041caac = iVar4;
  DAT_0041cab0 = iVar5;
  DAT_0041cac4 = *(undefined4 *)(param_1 + 0x1c);
  DAT_0041cac8 = *(undefined4 *)(param_1 + 0x20);
  DAT_0041cad4 = *(undefined4 *)(param_1 + 0x2c);
  DAT_0041cad8 = *(undefined4 *)(param_1 + 0x30);
  if (DAT_0041cac0 < iVar5) {
    DAT_0041caac = DAT_0041cabc;
    DAT_0041cab0 = DAT_0041cac0;
    DAT_0041cabc = iVar4;
    DAT_0041cac0 = iVar5;
    DAT_0041cac4 = *(undefined4 *)(param_1 + 0x2c);
    DAT_0041cac8 = *(undefined4 *)(param_1 + 0x30);
    DAT_0041cad4 = *(undefined4 *)(param_1 + 0x1c);
    DAT_0041cad8 = *(undefined4 *)(param_1 + 0x20);
  }
  if (iVar1 < DAT_0041cab0) {
    DAT_0041cab8 = DAT_0041cab0;
    DAT_0041cab4 = DAT_0041caac;
    DAT_0041cad0 = DAT_0041cac8;
    DAT_0041cacc = DAT_0041cac4;
    DAT_0041caac = iVar7;
    DAT_0041cab0 = iVar1;
    DAT_0041cac4 = uVar2;
    DAT_0041cac8 = uVar3;
  }
  else {
    DAT_0041cab4 = iVar7;
    DAT_0041cab8 = iVar1;
    DAT_0041cacc = uVar2;
    DAT_0041cad0 = uVar3;
    if (DAT_0041cac0 < iVar1) {
      DAT_0041cab8 = DAT_0041cac0;
      DAT_0041cab4 = DAT_0041cabc;
      DAT_0041cad0 = DAT_0041cad8;
      DAT_0041cacc = DAT_0041cad4;
      DAT_0041cabc = iVar7;
      DAT_0041cac0 = iVar1;
      DAT_0041cad4 = uVar2;
      DAT_0041cad8 = uVar3;
    }
  }
  if (DAT_0041cab0 < DAT_00453044) {
    FUN_00408750();
    return;
  }
  if (DAT_00453058 < DAT_0041cac0) {
    FUN_00408750();
    return;
  }
  if (DAT_0044ced8 <= DAT_00453064) {
    if (DAT_0044ceb8 < DAT_00453060) {
      FUN_00408750();
      return;
    }
    FUN_00456000();
    FUN_00407f80(DAT_0041cae0,DAT_0041cadc,DAT_0044cea8);
    iVar4 = FUN_00408d70(DAT_0041caac,DAT_0041cab0,DAT_0041cabc,DAT_0041cac0);
    iVar5 = FUN_00408d70(DAT_0041caac,DAT_0041cab0,DAT_0041cab4,DAT_0041cab8);
    _DAT_0044ce8c = DAT_0041cab0;
    _DAT_0044ce88 = DAT_0041caac;
    _DAT_0044ce90 = DAT_0041cab4;
    _DAT_0044ce94 = DAT_0041cab8;
    _DAT_0044ce98 = DAT_0041cac4;
    _DAT_0044ce9c = DAT_0041cac8;
    _DAT_0044cea0 = DAT_0041cacc;
    _DAT_0044cea4 = DAT_0041cad0;
    if (iVar5 < iVar4) {
      FUN_004561c0();
      _DAT_0044ce88 = DAT_0041caac;
      _DAT_0044ce8c = DAT_0041cab0;
      _DAT_0044ce90 = DAT_0041cabc;
      _DAT_0044ce94 = DAT_0041cac0;
      _DAT_0044ce98 = DAT_0041cac4;
      _DAT_0044ce9c = DAT_0041cac8;
      _DAT_0044cea0 = DAT_0041cad4;
      _DAT_0044cea4 = DAT_0041cad8;
      FUN_00456180();
      _DAT_0044ce88 = DAT_0041cab4;
      _DAT_0044ce8c = DAT_0041cab8;
      _DAT_0044ce90 = DAT_0041cabc;
      _DAT_0044ce94 = DAT_0041cac0;
      _DAT_0044ce98 = DAT_0041cacc;
      _DAT_0044ce9c = DAT_0041cad0;
      _DAT_0044cea0 = DAT_0041cad4;
      _DAT_0044cea4 = DAT_0041cad8;
      FUN_004561c0();
      iVar4 = 0;
      DAT_0044ceb4 = DAT_0044ceb4 + *DAT_0044cebc * DAT_0045305c;
      if (0 < DAT_0044cebc[1]) {
        iVar5 = 0;
        do {
          iVar7 = *(int *)((int)DAT_0044cebc + iVar5 + 0x10);
          iVar4 = iVar4 + 1;
          FUN_00407fa0(DAT_0044ceb4 + iVar7 + 1,
                       (*(int *)((int)DAT_0044cec0 + iVar5 + 0x10) - iVar7) + -1,
                       (*(uint *)((int)DAT_0044cec0 + iVar5 + 8) & 0xffff) +
                       *(int *)((int)DAT_0044cec0 + iVar5 + 0xc) * 0x1000000);
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
        } while (iVar4 < DAT_0044cebc[1]);
      }
      iVar5 = 0;
      if (0 < *(int *)(DAT_0044cec4 + 4)) {
        iVar7 = 0;
        iVar4 = iVar4 * 0xc;
        do {
          iVar1 = *(int *)(DAT_0044cec4 + iVar7 + 0x10);
          iVar7 = iVar7 + 0xc;
          iVar5 = iVar5 + 1;
          FUN_00407fa0(DAT_0044ceb4 + iVar1 + 1,
                       (*(int *)((int)DAT_0044cec0 + iVar4 + 0x10) - iVar1) + -1,
                       (*(uint *)((int)DAT_0044cec0 + iVar4 + 8) & 0xffff) +
                       *(int *)((int)DAT_0044cec0 + iVar4 + 0xc) * 0x1000000);
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
          iVar4 = iVar4 + 0xc;
        } while (iVar5 < *(int *)(DAT_0044cec4 + 4));
        return;
      }
    }
    else {
      FUN_00456180();
      _DAT_0044ce88 = DAT_0041caac;
      _DAT_0044ce8c = DAT_0041cab0;
      _DAT_0044ce90 = DAT_0041cabc;
      _DAT_0044ce94 = DAT_0041cac0;
      _DAT_0044ce98 = DAT_0041cac4;
      _DAT_0044ce9c = DAT_0041cac8;
      _DAT_0044cea0 = DAT_0041cad4;
      _DAT_0044cea4 = DAT_0041cad8;
      FUN_004561c0();
      _DAT_0044ce88 = DAT_0041cab4;
      _DAT_0044ce8c = DAT_0041cab8;
      _DAT_0044ce90 = DAT_0041cabc;
      _DAT_0044ce94 = DAT_0041cac0;
      _DAT_0044ce98 = DAT_0041cacc;
      _DAT_0044ce9c = DAT_0041cad0;
      _DAT_0044cea0 = DAT_0041cad4;
      _DAT_0044cea4 = DAT_0041cad8;
      FUN_00456180();
      iVar5 = 0;
      DAT_0044ceb4 = DAT_0044ceb4 + *DAT_0044cec0 * DAT_0045305c;
      iVar4 = 0;
      if (0 < DAT_0044cebc[1]) {
        do {
          iVar7 = *(int *)((int)DAT_0044cec0 + iVar4 + 0x10);
          iVar5 = iVar5 + 1;
          FUN_00407fa0(DAT_0044ceb4 + iVar7 + 1,
                       (*(int *)((int)DAT_0044cebc + iVar4 + 0x10) - iVar7) + -1,
                       (*(uint *)((int)DAT_0044cebc + iVar4 + 8) & 0xffff) +
                       *(int *)((int)DAT_0044cebc + iVar4 + 0xc) * 0x1000000);
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
          iVar4 = iVar4 + 0xc;
        } while (iVar5 < DAT_0044cebc[1]);
      }
      iVar7 = 0;
      iVar4 = 0;
      if (0 < *(int *)(DAT_0044cec4 + 4)) {
        iVar5 = iVar5 * 0xc;
        do {
          iVar6 = DAT_0044cec4 + iVar7;
          iVar1 = *(int *)((int)DAT_0044cec0 + iVar5 + 0x10);
          iVar7 = iVar7 + 0xc;
          iVar4 = iVar4 + 1;
          FUN_00407fa0(DAT_0044ceb4 + iVar1 + 1,(*(int *)(iVar6 + 0x10) - iVar1) + -1,
                       (*(uint *)(iVar6 + 8) & 0xffff) + *(int *)(iVar6 + 0xc) * 0x1000000);
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
        } while (iVar4 < *(int *)(DAT_0044cec4 + 4));
      }
    }
    return;
  }
  FUN_00408750();
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00408750(void)

{
  int iVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  
  if ((((DAT_00453044 <= DAT_0041cac0) && (DAT_0041cab0 <= DAT_00453058)) &&
      (DAT_0044ceb8 <= DAT_00453064)) && (DAT_00453060 <= DAT_0044ced8)) {
    FUN_00456000();
    FUN_00407f80(DAT_0041cae0,DAT_0041cadc,DAT_0044cea8);
    iVar1 = FUN_00408d70(DAT_0041cab4,DAT_0041cab8,DAT_0041cabc,DAT_0041cac0);
    iVar2 = FUN_00408d70(DAT_0041caac,DAT_0041cab0,DAT_0041cab4,DAT_0041cab8);
    _DAT_0044cee4 = DAT_0041cab0;
    _DAT_0044cee0 = DAT_0041caac;
    _DAT_0044cee8 = DAT_0041cab4;
    _DAT_0044ceec = DAT_0041cab8;
    _DAT_0044cef0 = DAT_0041cac4;
    _DAT_0044cef4 = DAT_0041cac8;
    _DAT_0044cef8 = DAT_0041cacc;
    _DAT_0044cefc = DAT_0041cad0;
    if (iVar2 < iVar1) {
      FUN_00456640();
      _DAT_0044cee0 = DAT_0041caac;
      _DAT_0044cee4 = DAT_0041cab0;
      _DAT_0044cee8 = DAT_0041cabc;
      _DAT_0044ceec = DAT_0041cac0;
      _DAT_0044cef0 = DAT_0041cac4;
      _DAT_0044cef4 = DAT_0041cac8;
      _DAT_0044cef8 = DAT_0041cad4;
      _DAT_0044cefc = DAT_0041cad8;
      FUN_00456520();
      _DAT_0044cee0 = DAT_0041cab4;
      _DAT_0044cee4 = DAT_0041cab8;
      _DAT_0044cee8 = DAT_0041cabc;
      _DAT_0044ceec = DAT_0041cac0;
      _DAT_0044cef0 = DAT_0041cacc;
      _DAT_0044cef4 = DAT_0041cad0;
      _DAT_0044cef8 = DAT_0041cad4;
      _DAT_0044cefc = DAT_0041cad8;
      FUN_00456640();
      iVar1 = 0;
      DAT_0044ceb4 = DAT_0044ceb4 + *DAT_0044cebc * DAT_0045305c;
      if (0 < DAT_0044cebc[1]) {
        iVar2 = 0;
        do {
          iVar5 = *(int *)((int)DAT_0044cebc + iVar2 + 0x10);
          if (iVar5 < DAT_00453054) {
            FUN_00407fc0(DAT_00453054 + DAT_0044ceb4,
                         *(int *)((int)DAT_0044cec0 + iVar2 + 0x10) - DAT_00453054,
                         (*(uint *)((int)DAT_0044cec0 + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044cec0 + iVar2 + 0xc) * 0x1000000);
          }
          else {
            FUN_00407fa0(DAT_0044ceb4 + iVar5 + 1,
                         (*(int *)((int)DAT_0044cec0 + iVar2 + 0x10) - iVar5) + -1,
                         (*(uint *)((int)DAT_0044cec0 + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044cec0 + iVar2 + 0xc) * 0x1000000);
          }
          iVar2 = iVar2 + 0xc;
          iVar1 = iVar1 + 1;
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
        } while (iVar1 < DAT_0044cebc[1]);
      }
      iVar2 = 0;
      if (0 < *(int *)(DAT_0044cec4 + 4)) {
        iVar5 = 0;
        iVar1 = iVar1 * 0xc;
        do {
          iVar3 = *(int *)(DAT_0044cec4 + iVar5 + 0x10);
          if (iVar3 < DAT_00453054) {
            FUN_00407fc0(DAT_00453054 + DAT_0044ceb4,
                         *(int *)((int)DAT_0044cec0 + iVar1 + 0x10) - DAT_00453054,
                         (*(uint *)((int)DAT_0044cec0 + iVar1 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044cec0 + iVar1 + 0xc) * 0x1000000);
          }
          else {
            FUN_00407fa0(DAT_0044ceb4 + iVar3 + 1,
                         (*(int *)((int)DAT_0044cec0 + iVar1 + 0x10) - iVar3) + -1,
                         (*(uint *)((int)DAT_0044cec0 + iVar1 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044cec0 + iVar1 + 0xc) * 0x1000000);
          }
          iVar1 = iVar1 + 0xc;
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
          iVar2 = iVar2 + 1;
        } while (iVar2 < *(int *)(DAT_0044cec4 + 4));
        return;
      }
    }
    else {
      FUN_00456520();
      _DAT_0044cee0 = DAT_0041caac;
      _DAT_0044cee4 = DAT_0041cab0;
      _DAT_0044cee8 = DAT_0041cabc;
      _DAT_0044ceec = DAT_0041cac0;
      _DAT_0044cef0 = DAT_0041cac4;
      _DAT_0044cef4 = DAT_0041cac8;
      _DAT_0044cef8 = DAT_0041cad4;
      _DAT_0044cefc = DAT_0041cad8;
      FUN_00456640();
      _DAT_0044cee0 = DAT_0041cab4;
      _DAT_0044cee4 = DAT_0041cab8;
      _DAT_0044cee8 = DAT_0041cabc;
      _DAT_0044ceec = DAT_0041cac0;
      _DAT_0044cef0 = DAT_0041cacc;
      _DAT_0044cef4 = DAT_0041cad0;
      _DAT_0044cef8 = DAT_0041cad4;
      _DAT_0044cefc = DAT_0041cad8;
      FUN_00456520();
      iVar2 = 0;
      iVar1 = 0;
      DAT_0044ceb4 = DAT_0044ceb4 + *DAT_0044cec0 * DAT_0045305c;
      if (0 < DAT_0044cebc[1]) {
        do {
          iVar5 = *(int *)((int)DAT_0044cec0 + iVar2 + 0x10);
          if (iVar5 < DAT_00453054) {
            FUN_00407fc0(DAT_00453054 + DAT_0044ceb4,
                         *(int *)((int)DAT_0044cebc + iVar2 + 0x10) - DAT_00453054,
                         (*(uint *)((int)DAT_0044cebc + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044cebc + iVar2 + 0xc) * 0x1000000);
          }
          else {
            FUN_00407fa0(DAT_0044ceb4 + iVar5 + 1,
                         (*(int *)((int)DAT_0044cebc + iVar2 + 0x10) - iVar5) + -1,
                         (*(uint *)((int)DAT_0044cebc + iVar2 + 8) & 0xffff) +
                         *(int *)((int)DAT_0044cebc + iVar2 + 0xc) * 0x1000000);
          }
          iVar2 = iVar2 + 0xc;
          iVar1 = iVar1 + 1;
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
        } while (iVar1 < DAT_0044cebc[1]);
      }
      iVar5 = 0;
      iVar2 = 0;
      if (0 < *(int *)(DAT_0044cec4 + 4)) {
        iVar1 = iVar1 * 0xc;
        do {
          iVar3 = *(int *)((int)DAT_0044cec0 + iVar1 + 0x10);
          if (iVar3 < DAT_00453054) {
            iVar3 = DAT_0044cec4 + iVar5;
            FUN_00407fc0(DAT_00453054 + DAT_0044ceb4,*(int *)(iVar3 + 0x10) - DAT_00453054,
                         (*(uint *)(iVar3 + 8) & 0xffff) + *(int *)(iVar3 + 0xc) * 0x1000000);
          }
          else {
            iVar4 = DAT_0044cec4 + iVar5;
            FUN_00407fa0(DAT_0044ceb4 + iVar3 + 1,(*(int *)(iVar4 + 0x10) - iVar3) + -1,
                         (*(uint *)(iVar4 + 8) & 0xffff) + *(int *)(iVar4 + 0xc) * 0x1000000);
          }
          iVar1 = iVar1 + 0xc;
          DAT_0044ceb4 = DAT_0044ceb4 + DAT_0045305c;
          iVar5 = iVar5 + 0xc;
          iVar2 = iVar2 + 1;
        } while (iVar2 < *(int *)(DAT_0044cec4 + 4));
      }
    }
  }
  return;
}



int __cdecl FUN_00408d70(int param_1,int param_2,int param_3,int param_4)

{
  int iVar1;
  
  if (param_4 == param_2) {
    iVar1 = -0x7fff0000;
    if (param_1 <= param_3) {
      return 0x7fff0000;
    }
  }
  else {
    iVar1 = ((param_1 - param_3) * 0x100) / (param_2 - param_4);
  }
  return iVar1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined8 __fastcall FUN_00408dc9(undefined4 param_1,undefined4 param_2)

{
  undefined1 uVar1;
  int *piVar2;
  uint3 uVar3;
  byte bVar4;
  undefined4 in_EAX;
  undefined2 uVar5;
  int iVar6;
  int iVar7;
  byte *pbVar8;
  undefined2 uVar9;
  int iVar10;
  int iVar11;
  int iVar12;
  byte *pbVar13;
  byte *pbVar14;
  undefined2 uVar15;
  int unaff_ESI;
  undefined1 *puVar16;
  
  if (*(int *)(unaff_ESI + 0xc) != 0) {
    _DAT_0041ca74 = **(undefined4 **)(unaff_ESI + 4);
    _DAT_0041ca78 = (*(undefined4 **)(unaff_ESI + 4))[1];
    _DAT_0041ca6c = *(undefined4 *)(unaff_ESI + 8);
    _DAT_0041ca70 = *(undefined4 *)(unaff_ESI + 0xc);
    FUN_004091f8();
    return CONCAT44(param_2,in_EAX);
  }
  piVar2 = *(int **)(unaff_ESI + 8);
  DAT_0041ca5c = (undefined1 *)
                 (CONCAT31((int3)((uint)piVar2[3] >> 8),*(undefined1 *)((int)piVar2 + 9)) +
                 piVar2[6]);
  iVar10 = (piVar2[4] >> 8) - (piVar2[2] >> 8);
  iVar11 = (piVar2[5] >> 8) - (piVar2[3] >> 8);
  iVar12 = **(int **)(unaff_ESI + 4) - *piVar2 >> 8;
  iVar6 = (*(int **)(unaff_ESI + 4))[1] - piVar2[1] >> 8;
  DAT_0041ca64 = iVar11;
  DAT_0041ca60 = iVar10;
  if (iVar6 < DAT_0045304c) {
    iVar6 = DAT_0045304c - iVar6;
    DAT_0041ca64 = iVar11 - iVar6;
    if (DAT_0041ca64 == 0 || iVar11 < iVar6) goto LAB_00408f7c;
    DAT_0041ca5c = DAT_0041ca5c + iVar6 * 0x100;
    iVar6 = DAT_0045304c;
  }
  iVar11 = DAT_0041ca64;
  iVar7 = (iVar6 + DAT_0041ca64) - DAT_00453048;
  if ((iVar7 == 0 || iVar6 + DAT_0041ca64 < DAT_00453048) ||
     (DAT_0041ca64 = DAT_0041ca64 - iVar7, DAT_0041ca64 != 0 && iVar7 <= iVar11)) {
    if (iVar12 < DAT_00453054) {
      iVar12 = DAT_00453054 - iVar12;
      DAT_0041ca60 = iVar10 - iVar12;
      if (DAT_0041ca60 == 0 || iVar10 < iVar12) goto LAB_00408f7c;
      DAT_0041ca5c = DAT_0041ca5c + iVar12;
      iVar12 = DAT_00453054;
    }
    iVar10 = DAT_0041ca60;
    iVar11 = (iVar12 + DAT_0041ca60) - _DAT_00453050;
    if ((iVar11 == 0 || iVar12 + DAT_0041ca60 < _DAT_00453050) ||
       (DAT_0041ca60 = DAT_0041ca60 - iVar11, DAT_0041ca60 != 0 && iVar11 <= iVar10)) {
      pbVar13 = (byte *)(iVar12 + iVar6 * DAT_0045305c + DAT_00453068);
      pbVar8 = *(byte **)(*(int *)(unaff_ESI + 8) + 0x1c);
      pbVar14 = pbVar8;
      iVar6 = DAT_0041ca64;
      puVar16 = DAT_0041ca5c;
      do {
        iVar12 = DAT_0041ca60 + -6;
        uVar5 = (undefined2)((uint)pbVar8 >> 0x10);
        uVar9 = (undefined2)((uint)pbVar14 >> 0x10);
        if (DAT_0041ca60 < 6) {
          iVar12 = DAT_0041ca60 + -2;
          if (-1 < iVar12) {
            pbVar14 = (byte *)CONCAT22(uVar9,CONCAT11(puVar16[DAT_0041ca60 + -1],
                                                      pbVar13[DAT_0041ca60 + -1]));
            pbVar8 = (byte *)CONCAT31(CONCAT21(uVar5,puVar16[iVar12]),pbVar13[iVar12]);
            iVar12 = DAT_0041ca60 + -6;
            bVar4 = *pbVar14;
            goto LAB_00408f4a;
          }
          if (-1 < DAT_0041ca60 + -1) {
            pbVar8 = (byte *)CONCAT22(uVar5,CONCAT11(*puVar16,*pbVar13));
            bVar4 = *pbVar8;
            goto LAB_00408f64;
          }
        }
        else {
          pbVar14 = (byte *)CONCAT22(uVar9,CONCAT11(puVar16[DAT_0041ca60 + -1],
                                                    pbVar13[DAT_0041ca60 + -1]));
          pbVar8 = (byte *)CONCAT31(CONCAT21(uVar5,puVar16[DAT_0041ca60 + -2]),
                                    pbVar13[DAT_0041ca60 + -2]);
          uVar3 = (uint3)*pbVar14;
          do {
            uVar15 = (undefined2)((uint)pbVar14 >> 0x10);
            uVar9 = (undefined2)((uint)pbVar8 >> 0x10);
            uVar5 = (undefined2)CONCAT31(uVar3,*pbVar8);
            pbVar14 = (byte *)CONCAT22(uVar15,CONCAT11(puVar16[iVar12 + 1],pbVar13[iVar12 + 1]));
            *(uint *)(pbVar13 + iVar12 + 2) =
                 CONCAT31(CONCAT21(uVar5,*(undefined1 *)
                                          CONCAT22(uVar15,CONCAT11(puVar16[iVar12 + 3],
                                                                   pbVar13[iVar12 + 3]))),
                          *(undefined1 *)
                           CONCAT22(uVar9,CONCAT11(puVar16[iVar12 + 2],pbVar13[iVar12 + 2])));
            pbVar8 = (byte *)CONCAT22(uVar9,CONCAT11(puVar16[iVar12],pbVar13[iVar12]));
            iVar12 = iVar12 + -4;
            bVar4 = *pbVar14;
            uVar3 = CONCAT21(uVar5,bVar4);
          } while (-1 < iVar12);
LAB_00408f4a:
          pbVar13[iVar12 + 5] = bVar4;
          bVar4 = *pbVar8;
          for (iVar12 = iVar12 + 3; -1 < iVar12; iVar12 = iVar12 + -1) {
            uVar1 = puVar16[iVar12];
            pbVar13[iVar12 + 1] = bVar4;
            pbVar8 = (byte *)CONCAT31(CONCAT21((short)((uint)pbVar8 >> 0x10),uVar1),pbVar13[iVar12])
            ;
            bVar4 = *pbVar8;
          }
LAB_00408f64:
          *pbVar13 = bVar4;
        }
        puVar16 = puVar16 + 0x100;
        pbVar13 = pbVar13 + DAT_0045305c;
        iVar6 = iVar6 + -1;
      } while (iVar6 != 0);
    }
  }
LAB_00408f7c:
  return CONCAT44(param_2,in_EAX);
}



void __fastcall FUN_00408fa4(undefined4 param_1,undefined4 param_2)

{
  int in_EAX;
  int unaff_EBX;
  
  DAT_0041caa4 = param_2;
  DAT_0041caa0 = param_1;
  DAT_0041ca98 = (char)((uint)-unaff_EBX >> 0x10);
  DAT_0041ca9c = CONCAT22((short)((uint)(unaff_EBX * -0x10000) >> 0x10),(short)((uint)-in_EAX >> 8))
  ;
  return;
}



undefined1 * __fastcall FUN_00408fca(uint param_1)

{
  char cVar1;
  uint uVar2;
  undefined1 *in_EAX;
  undefined1 *puVar3;
  undefined2 unaff_BX;
  undefined2 uVar5;
  undefined1 *puVar4;
  int unaff_ESI;
  int iVar6;
  int unaff_EDI;
  bool bVar7;
  
  uVar2 = DAT_0041ca9c;
  cVar1 = DAT_0041ca98;
  uVar5 = (undefined2)((uint)DAT_0041caa0 >> 0x10);
  DAT_0041caa8 = *(undefined1 *)CONCAT22(uVar5,CONCAT11(*in_EAX,in_EAX[4]));
  puVar4 = (undefined1 *)CONCAT22(uVar5,unaff_BX);
  puVar3 = DAT_0041caa4;
  for (iVar6 = unaff_ESI + -1; uVar5 = (undefined2)((uint)puVar3 >> 0x10), -1 < iVar6;
      iVar6 = iVar6 + -1) {
    bVar7 = CARRY4(param_1,uVar2);
    param_1 = param_1 + uVar2;
    puVar4 = (undefined1 *)
             CONCAT31((int3)(CONCAT22((short)((uint)puVar4 >> 0x10),
                                      CONCAT11((char)(param_1 >> 8),(char)puVar4)) >> 8),
                      (char)puVar4 + cVar1 + bVar7);
    puVar3 = (undefined1 *)CONCAT22(uVar5,CONCAT11(*puVar4,*(undefined1 *)(unaff_EDI + iVar6)));
    *(undefined1 *)(unaff_EDI + iVar6) = *puVar3;
  }
  if (-1 < iVar6 + 1) {
    puVar3 = (undefined1 *)CONCAT31(CONCAT21(uVar5,DAT_0041caa8),*(undefined1 *)(unaff_EDI + iVar6))
    ;
    *(undefined1 *)(unaff_EDI + iVar6) = *puVar3;
  }
  return puVar3;
}



undefined1 * __fastcall FUN_0040901f(uint param_1)

{
  char cVar1;
  uint uVar2;
  undefined1 *puVar3;
  undefined2 unaff_BX;
  undefined1 *puVar4;
  int unaff_ESI;
  int iVar5;
  int unaff_EDI;
  bool bVar6;
  
  uVar2 = DAT_0041ca9c;
  cVar1 = DAT_0041ca98;
  puVar4 = (undefined1 *)CONCAT22((short)((uint)DAT_0041caa0 >> 0x10),unaff_BX);
  puVar3 = DAT_0041caa4;
  for (iVar5 = unaff_ESI + -1; -1 < iVar5; iVar5 = iVar5 + -1) {
    bVar6 = CARRY4(param_1,uVar2);
    param_1 = param_1 + uVar2;
    puVar4 = (undefined1 *)
             CONCAT31((int3)(CONCAT22((short)((uint)puVar4 >> 0x10),
                                      CONCAT11((char)(param_1 >> 8),(char)puVar4)) >> 8),
                      (char)puVar4 + cVar1 + bVar6);
    puVar3 = (undefined1 *)
             CONCAT31(CONCAT21((short)((uint)puVar3 >> 0x10),*puVar4),
                      *(undefined1 *)(unaff_EDI + iVar5));
    *(undefined1 *)(unaff_EDI + iVar5) = *puVar3;
  }
  return puVar3;
}



void __fastcall FUN_004090b8(undefined4 param_1)

{
  int in_EAX;
  int unaff_EBX;
  
  DAT_0041cc3c = param_1;
  DAT_0041cc34 = (char)((uint)-unaff_EBX >> 0x10);
  DAT_0041cc38 = CONCAT22((short)((uint)(unaff_EBX * -0x10000) >> 0x10),(short)((uint)-in_EAX >> 8))
  ;
  return;
}



void __fastcall FUN_004090d8(uint param_1)

{
  byte bVar1;
  char cVar2;
  uint uVar3;
  undefined1 *in_EAX;
  uint uVar4;
  uint uVar5;
  uint uVar6;
  char cVar7;
  char cVar8;
  char cVar9;
  char cVar10;
  undefined2 unaff_BX;
  undefined2 uVar12;
  undefined1 *puVar11;
  int unaff_ESI;
  int iVar13;
  int unaff_EDI;
  bool bVar14;
  
  uVar3 = DAT_0041cc38;
  cVar2 = DAT_0041cc34;
  uVar12 = (undefined2)((uint)DAT_0041cc3c >> 0x10);
  DAT_0041cc44 = *(undefined1 *)CONCAT22(uVar12,CONCAT11(*in_EAX,in_EAX[4]));
  puVar11 = (undefined1 *)CONCAT22(uVar12,unaff_BX);
  for (iVar13 = unaff_ESI + -4; -1 < iVar13; iVar13 = iVar13 + -4) {
    uVar4 = param_1 + uVar3;
    uVar12 = (undefined2)((uint)puVar11 >> 0x10);
    cVar7 = (char)puVar11;
    cVar8 = cVar7 + cVar2 + CARRY4(param_1,uVar3);
    uVar5 = uVar4 + uVar3;
    cVar9 = cVar8 + cVar2 + CARRY4(uVar4,uVar3);
    uVar6 = uVar5 + uVar3;
    bVar1 = *(byte *)CONCAT31((int3)(CONCAT22(uVar12,CONCAT11((char)(uVar5 >> 8),cVar8)) >> 8),cVar9
                             );
    cVar10 = cVar9 + cVar2 + CARRY4(uVar5,uVar3);
    param_1 = uVar6 + uVar3;
    puVar11 = (undefined1 *)
              CONCAT31((int3)(CONCAT22(uVar12,CONCAT11((char)(param_1 >> 8),cVar10)) >> 8),
                       cVar10 + cVar2 + CARRY4(uVar6,uVar3));
    *(uint *)(unaff_EDI + iVar13) =
         CONCAT31(CONCAT21((ushort)bVar1 |
                           (ushort)(((uint)CONCAT11(bVar1,*(undefined1 *)
                                                           CONCAT31((int3)(CONCAT22(uVar12,CONCAT11(
                                                  (char)(uVar4 >> 8),cVar7)) >> 8),cVar8)) << 0x18)
                                   >> 0x10),
                           *(undefined1 *)
                            CONCAT31((int3)(CONCAT22(uVar12,CONCAT11((char)(uVar6 >> 8),cVar9)) >> 8
                                           ),cVar10)),*puVar11);
  }
  for (iVar13 = iVar13 + 3; -1 < iVar13; iVar13 = iVar13 + -1) {
    bVar14 = CARRY4(param_1,uVar3);
    param_1 = param_1 + uVar3;
    puVar11 = (undefined1 *)
              CONCAT31((int3)(CONCAT22((short)((uint)puVar11 >> 0x10),
                                       CONCAT11((char)(param_1 >> 8),(char)puVar11)) >> 8),
                       (char)puVar11 + cVar2 + bVar14);
    *(undefined1 *)(unaff_EDI + iVar13) = *puVar11;
  }
  if (-1 < iVar13 + 1) {
    *(undefined1 *)(unaff_EDI + iVar13) = DAT_0041cc44;
  }
  return;
}



void __fastcall FUN_0040914e(uint param_1)

{
  byte bVar1;
  char cVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  uint uVar6;
  char cVar7;
  char cVar8;
  char cVar9;
  char cVar10;
  undefined2 unaff_BX;
  undefined2 uVar12;
  undefined1 *puVar11;
  int unaff_ESI;
  int iVar13;
  int unaff_EDI;
  bool bVar14;
  
  uVar3 = DAT_0041cc38;
  cVar2 = DAT_0041cc34;
  puVar11 = (undefined1 *)CONCAT22((short)((uint)DAT_0041cc3c >> 0x10),unaff_BX);
  for (iVar13 = unaff_ESI + -4; -1 < iVar13; iVar13 = iVar13 + -4) {
    uVar4 = param_1 + uVar3;
    uVar12 = (undefined2)((uint)puVar11 >> 0x10);
    cVar7 = (char)puVar11;
    cVar8 = cVar7 + cVar2 + CARRY4(param_1,uVar3);
    uVar5 = uVar4 + uVar3;
    cVar9 = cVar8 + cVar2 + CARRY4(uVar4,uVar3);
    uVar6 = uVar5 + uVar3;
    bVar1 = *(byte *)CONCAT31((int3)(CONCAT22(uVar12,CONCAT11((char)(uVar5 >> 8),cVar8)) >> 8),cVar9
                             );
    cVar10 = cVar9 + cVar2 + CARRY4(uVar5,uVar3);
    param_1 = uVar6 + uVar3;
    puVar11 = (undefined1 *)
              CONCAT31((int3)(CONCAT22(uVar12,CONCAT11((char)(param_1 >> 8),cVar10)) >> 8),
                       cVar10 + cVar2 + CARRY4(uVar6,uVar3));
    *(uint *)(unaff_EDI + iVar13) =
         CONCAT31(CONCAT21((ushort)bVar1 |
                           (ushort)(((uint)CONCAT11(bVar1,*(undefined1 *)
                                                           CONCAT31((int3)(CONCAT22(uVar12,CONCAT11(
                                                  (char)(uVar4 >> 8),cVar7)) >> 8),cVar8)) << 0x18)
                                   >> 0x10),
                           *(undefined1 *)
                            CONCAT31((int3)(CONCAT22(uVar12,CONCAT11((char)(uVar6 >> 8),cVar9)) >> 8
                                           ),cVar10)),*puVar11);
  }
  for (iVar13 = iVar13 + 3; -1 < iVar13; iVar13 = iVar13 + -1) {
    bVar14 = CARRY4(param_1,uVar3);
    param_1 = param_1 + uVar3;
    puVar11 = (undefined1 *)
              CONCAT31((int3)(CONCAT22((short)((uint)puVar11 >> 0x10),
                                       CONCAT11((char)(param_1 >> 8),(char)puVar11)) >> 8),
                       (char)puVar11 + cVar2 + bVar14);
    *(undefined1 *)(unaff_EDI + iVar13) = *puVar11;
  }
  return;
}



undefined * FUN_004091f8(void)

{
  int *piVar1;
  int *piVar2;
  longlong lVar3;
  ushort uVar8;
  ushort uVar9;
  uint uVar4;
  uint uVar5;
  int iVar6;
  int iVar7;
  int iVar10;
  int iVar11;
  int unaff_ESI;
  undefined *puVar12;
  
  piVar1 = *(int **)(unaff_ESI + 4);
  DAT_0041cc68 = piVar1[3];
  DAT_0041cc6c = piVar1[5] + -0x10;
  DAT_0041cc70 = piVar1[2];
  DAT_0041cc74 = piVar1[4] + -0x10;
  DAT_0041cc78 = piVar1[7];
  DAT_0041cc7c = piVar1[6];
  piVar2 = *(int **)(unaff_ESI + 8);
  uVar8 = (ushort)((ulonglong)((longlong)-*piVar1 * (longlong)*piVar2) >> 0x10);
  uVar9 = (ushort)((ulonglong)((longlong)-piVar1[1] * (longlong)piVar2[2]) >> 0x10);
  DAT_0041cc48 = (CONCAT22(uVar8,(short)((ulonglong)((longlong)-*piVar1 * (longlong)*piVar2) >> 0x20
                                        )) << 0x10 | (uint)uVar8) +
                 (CONCAT22(uVar9,(short)((ulonglong)((longlong)-piVar1[1] * (longlong)piVar2[2]) >>
                                        0x20)) << 0x10 | (uint)uVar9);
  uVar8 = (ushort)((ulonglong)((longlong)-*piVar1 * (longlong)piVar2[1]) >> 0x10);
  uVar9 = (ushort)((ulonglong)((longlong)-piVar1[1] * (longlong)piVar2[3]) >> 0x10);
  DAT_0041cc4c = (CONCAT22(uVar8,(short)((ulonglong)((longlong)-*piVar1 * (longlong)piVar2[1]) >>
                                        0x20)) << 0x10 | (uint)uVar8) +
                 (CONCAT22(uVar9,(short)((ulonglong)((longlong)-piVar1[1] * (longlong)piVar2[3]) >>
                                        0x20)) << 0x10 | (uint)uVar9);
  iVar10 = (piVar1[4] + -0x10) - piVar1[2];
  lVar3 = (longlong)piVar2[1] * (longlong)iVar10;
  uVar8 = (ushort)((ulonglong)lVar3 >> 0x10);
  uVar4 = CONCAT22(uVar8,(short)((ulonglong)lVar3 >> 0x20)) << 0x10 | (uint)uVar8;
  lVar3 = (longlong)*piVar2 * (longlong)iVar10;
  uVar8 = (ushort)((ulonglong)lVar3 >> 0x10);
  uVar5 = CONCAT22(uVar8,(short)((ulonglong)lVar3 >> 0x20)) << 0x10 | (uint)uVar8;
  iVar11 = (piVar1[5] + -0x10) - piVar1[3];
  lVar3 = (longlong)piVar2[3] * (longlong)iVar11;
  uVar8 = (ushort)((ulonglong)lVar3 >> 0x10);
  iVar10 = (CONCAT22(uVar8,(short)((ulonglong)lVar3 >> 0x20)) << 0x10 | (uint)uVar8) + DAT_0041cc4c;
  iVar6 = uVar4 + DAT_0041cc4c;
  lVar3 = (longlong)piVar2[2] * (longlong)iVar11;
  uVar8 = (ushort)((ulonglong)lVar3 >> 0x10);
  iVar11 = (CONCAT22(uVar8,(short)((ulonglong)lVar3 >> 0x20)) << 0x10 | (uint)uVar8) + DAT_0041cc48;
  iVar7 = uVar5 + DAT_0041cc48;
  DAT_0041cc48 = DAT_0041cc48 + *(int *)(unaff_ESI + 0xc);
  DAT_0041cc4c = DAT_0041cc4c + *(int *)(unaff_ESI + 0x10);
  DAT_0041cc50 = iVar7 + *(int *)(unaff_ESI + 0xc);
  DAT_0041cc54 = iVar6 + *(int *)(unaff_ESI + 0x10);
  DAT_0041cc58 = iVar11 + *(int *)(unaff_ESI + 0xc);
  DAT_0041cc5c = iVar10 + *(int *)(unaff_ESI + 0x10);
  DAT_0041cc60 = iVar11 + uVar5 + *(int *)(unaff_ESI + 0xc);
  DAT_0041cc64 = iVar10 + uVar4 + *(int *)(unaff_ESI + 0x10);
  DAT_0041cc84 = DAT_0041cc48;
  DAT_0041cc88 = DAT_0041cc4c;
  DAT_0041cc8c = DAT_0041cc50;
  DAT_0041cc90 = DAT_0041cc54;
  DAT_0041cc94 = DAT_0041cc58;
  DAT_0041cc98 = DAT_0041cc5c;
  DAT_0041cc9c = DAT_0041cc70;
  DAT_0041cca0 = DAT_0041cc68;
  DAT_0041cca4 = DAT_0041cc74;
  DAT_0041cca8 = DAT_0041cc68;
  DAT_0041ccac = DAT_0041cc70;
  DAT_0041ccb0 = DAT_0041cc6c;
  DAT_0041ccb4 = DAT_0041cc7c;
  DAT_0041ccb8 = DAT_0041cc78;
  FUN_004071f0(0x41cc80);
  DAT_0041cc84 = DAT_0041cc50;
  DAT_0041cc88 = DAT_0041cc54;
  DAT_0041cc8c = DAT_0041cc58;
  DAT_0041cc90 = DAT_0041cc5c;
  DAT_0041cc94 = DAT_0041cc60;
  DAT_0041cc98 = DAT_0041cc64;
  DAT_0041cca0 = DAT_0041cc68;
  DAT_0041cca8 = DAT_0041cc6c;
  DAT_0041ccb0 = DAT_0041cc6c;
  DAT_0041cc9c = DAT_0041cc74;
  DAT_0041ccac = DAT_0041cc74;
  DAT_0041cca4 = DAT_0041cc70;
  DAT_0041ccb4 = DAT_0041cc7c;
  DAT_0041ccb8 = DAT_0041cc78;
  puVar12 = &DAT_0041cc80;
  FUN_004071f0(0x41cc80);
  return puVar12;
}



void DirectDrawCreate(void)

{
                    // WARNING: Could not recover jumptable at 0x00409470. Too many branches
                    // WARNING: Treating indirect jump as call
  DirectDrawCreate();
  return;
}



void DirectSoundCreate(void)

{
                    // WARNING: Could not recover jumptable at 0x00409476. Too many branches
                    // WARNING: Treating indirect jump as call
  DirectSoundCreate();
  return;
}



void DirectInputCreateA(void)

{
                    // WARNING: Could not recover jumptable at 0x0040a498. Too many branches
                    // WARNING: Treating indirect jump as call
  DirectInputCreateA();
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

longlong __fastcall FUN_0040a4a0(undefined4 param_1,uint param_2)

{
  int in_EAX;
  short sVar1;
  int unaff_EBX;
  uint uVar2;
  int iVar3;
  undefined4 unaff_ESI;
  int *piVar4;
  undefined **ppuVar5;
  
  if ((in_EAX < 0x579) && (unaff_EBX < 0x259)) {
    piVar4 = &DAT_0041d75c;
    uVar2 = 1;
    DAT_0041cdf0 = in_EAX;
    _DAT_0041cdf4 = unaff_EBX;
    do {
      *piVar4 = (int)(0x10000 / (ulonglong)uVar2) + -1;
      piVar4 = piVar4 + 1;
      uVar2 = uVar2 + 1;
    } while (uVar2 != 0x3a9b);
    for (ppuVar5 = &PTR_FUN_0042c21c; *ppuVar5 != (undefined *)0x0; ppuVar5 = ppuVar5 + 1) {
      (*(code *)*ppuVar5)(ppuVar5,unaff_ESI,unaff_EBX);
    }
    piVar4 = &DAT_0041cdf8;
    iVar3 = 0;
    sVar1 = 600;
    do {
      *piVar4 = iVar3;
      piVar4 = piVar4 + 1;
      iVar3 = iVar3 + in_EAX;
      sVar1 = sVar1 + -1;
    } while (sVar1 != 0);
    return (ulonglong)param_2 << 0x20;
  }
  return CONCAT44(param_2,0xffffffff);
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

longlong __fastcall FUN_0040a519(undefined4 param_1,uint param_2)

{
  undefined4 *unaff_ESI;
  longlong lVar1;
  
  _DAT_0041cd98 = unaff_ESI;
  _DAT_0041cd9c = (int *)*unaff_ESI;
  _DAT_0041cda4 = unaff_ESI[1];
  _DAT_0041cdac = unaff_ESI[3];
  _DAT_0041cdb0 = unaff_ESI[4];
  _DAT_0041cdb4 = unaff_ESI[5];
  _DAT_0041cdc0 = _DAT_0041cdb0;
  _DAT_0041cdd0 = _DAT_0041cdb0 << 8;
  _DAT_0041cdc4 = _DAT_0041cdb4;
  _DAT_0041cdd4 = _DAT_0041cdb4 << 8;
  DAT_0041cde0 = _DAT_0041cdd0;
  DAT_0041cde4 = _DAT_0041cdd4;
  _DAT_0041cdb8 = unaff_ESI[6];
  _DAT_0041cdbc = unaff_ESI[7];
  _DAT_0041cdc8 = _DAT_0041cdb8 + 1;
  _DAT_0041cdcc = _DAT_0041cdbc + 1;
  DAT_0041cde8 = _DAT_0041cdc8 * 0x100;
  _DAT_0041cdd8 = DAT_0041cde8 + -1;
  DAT_0041cdec = _DAT_0041cdcc * 0x100;
  _DAT_0041cddc = DAT_0041cdec + -1;
  _DAT_0041cda0 = (int *)*_DAT_0041cd9c;
  if (_DAT_0041cda0 != (int *)0x0) {
                    // WARNING: Could not recover jumptable at 0x0040a5bf. Too many branches
                    // WARNING: Treating indirect jump as call
    lVar1 = (*(code *)(&PTR_DAT_0042c1c4)[*_DAT_0041cda0])();
    return lVar1;
  }
  return (ulonglong)param_2 << 0x20;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_0040c650(void)

{
  float fVar1;
  float fVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  
  _DAT_00436728 = 0.0;
  iVar6 = DAT_0045352c - DAT_00453504;
  if (iVar6 != 0) {
    _DAT_00436728 = (float)(DAT_00453528 - DAT_00453500) / (float)iVar6;
  }
  _DAT_0043672c = 0.0;
  DAT_004534dc = DAT_00453518 - DAT_00453504;
  if (DAT_004534dc != 0) {
    _DAT_0043672c = (float)(DAT_00453514 - DAT_00453500) / (float)DAT_004534dc;
  }
  DAT_00453554 = 0.0;
  DAT_004534e0 = DAT_0045352c - DAT_00453518;
  if (DAT_004534e0 != 0) {
    DAT_00453554 = (float)(DAT_00453528 - DAT_00453514) / (float)DAT_004534e0;
  }
  _DAT_00436740 = DAT_00453534 - DAT_0045350c;
  _DAT_00436744 = DAT_00453538 - DAT_00453510;
  DAT_00436750 = DAT_00453530 - DAT_00453508;
  _DAT_00436748 = DAT_00453520 - DAT_0045350c;
  _DAT_0043674c = DAT_00453524 - DAT_00453510;
  DAT_00436754 = DAT_0045351c - DAT_00453508;
  DAT_004534ec = DAT_00453534 - DAT_00453520;
  DAT_004534f0 = DAT_00453538 - DAT_00453524;
  DAT_004534d8 = DAT_00453530 - DAT_0045351c;
  iVar7 = DAT_00453528 - DAT_00453500;
  if (iVar7 < 0) {
    iVar7 = DAT_00453500 - DAT_00453528;
  }
  iVar3 = iVar6;
  if (iVar6 < 0) {
    iVar3 = DAT_00453504 - DAT_0045352c;
  }
  if (iVar7 <= iVar3) {
    iVar7 = iVar3;
  }
  iVar3 = DAT_00453514 - DAT_00453500;
  if (iVar3 < 0) {
    iVar3 = DAT_00453500 - DAT_00453514;
  }
  iVar4 = DAT_004534dc;
  if (DAT_004534dc < 0) {
    iVar4 = DAT_00453504 - DAT_00453518;
  }
  if (iVar3 <= iVar4) {
    iVar3 = iVar4;
  }
  iVar4 = DAT_00453528 - DAT_00453514;
  if (iVar4 < 0) {
    iVar4 = DAT_00453514 - DAT_00453528;
  }
  iVar5 = DAT_004534e0;
  if (DAT_004534e0 < 0) {
    iVar5 = DAT_00453518 - DAT_0045352c;
  }
  if (iVar4 <= iVar5) {
    iVar4 = iVar5;
  }
  fVar2 = (float)DAT_00453508;
  fVar1 = (float)DAT_0045351c;
  _DAT_00436730 = ((float)iVar7 / fVar2) * (float)DAT_00453530;
  _DAT_00436738 = ((float)iVar3 / fVar2) * fVar1;
  DAT_00453560 = ((float)iVar4 / fVar1) * (float)DAT_00453530;
  _DAT_00436734 = (float)-(DAT_00453530 - DAT_00453508) / fVar2;
  _DAT_0043673c = (float)-(DAT_0045351c - DAT_00453508) / fVar2;
  DAT_0045355c = (float)-(DAT_00453530 - DAT_0045351c) / fVar1;
  if (DAT_0045352c != DAT_00453504) {
    _DAT_00436704 = (float)iVar7 / (float)iVar6;
  }
  if (DAT_00453518 != DAT_00453504) {
    _DAT_00436710 = (float)iVar3 / (float)DAT_004534dc;
  }
  if (DAT_0045352c != DAT_00453518) {
    DAT_00453558 = (float)iVar4 / (float)DAT_004534e0;
  }
  DAT_004534e4 = DAT_0045354c - DAT_00453548;
  DAT_004534e8 = DAT_00453550 - DAT_0045354c;
  _DAT_00436758 = (uint)(_DAT_0043672c <= _DAT_00436728);
  if ((DAT_004534dc == 0) && (_DAT_00436758 = 0, DAT_00453514 < DAT_00453500)) {
    _DAT_00436758 = 1;
  }
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_0040cb70(void)

{
  int iVar1;
  
  DAT_004534f4 = DAT_0041cde4 - DAT_00453504;
  if (DAT_004534f4 < 0) {
    DAT_004534f4 = 0;
  }
  DAT_004534f8 = DAT_0045352c - DAT_0041cdec;
  if (DAT_004534f8 < 0) {
    DAT_004534f8 = 0;
  }
  if ((DAT_004534f4 < DAT_004534dc) && (DAT_004534e4 != 0)) {
    if (DAT_004534e0 < DAT_004534f8) {
      DAT_004534dc = (DAT_004534dc - DAT_004534f8) + DAT_004534e0;
    }
    iVar1 = DAT_004534f4;
    if (DAT_004534f4 == 0) {
      iVar1 = -(DAT_00453504 & 0xff);
    }
    DAT_004366fc = iVar1 + 0x100;
    _DAT_0043670c = (float)DAT_004366fc * _DAT_00436710;
    _DAT_00436700 = (float)DAT_004366fc * _DAT_00436704;
    _DAT_00436714 = DAT_00453500;
    _DAT_00436718 = DAT_00453504;
    _DAT_0043671c = DAT_00453508;
    _DAT_00436720 = DAT_0045350c;
    _DAT_00436724 = DAT_00453510;
    DAT_004366f8 = DAT_004534dc;
    DAT_00436708 = DAT_004366fc;
    FUN_0040e9cc();
  }
  if ((DAT_004534f8 <= DAT_004534e0) && (DAT_004534e8 != 0)) {
    _DAT_00436710 = DAT_00453558;
    DAT_00436708 = 0x100 - (DAT_00453518 & 0xff);
    if (DAT_00436708 == 0x100) {
      DAT_00436708 = 0;
    }
    DAT_004366fc = DAT_004534dc + DAT_00436708;
    if (DAT_004534dc < DAT_004534f4) {
      DAT_004366fc = DAT_004534f4 + 0x100;
      DAT_00436708 = (DAT_004534f4 - DAT_004534dc) + 0x100;
    }
    _DAT_00436700 = (float)DAT_004366fc * _DAT_00436704;
    _DAT_0043670c = (float)DAT_00436708 * DAT_00453558;
    DAT_004366f8 = DAT_004534e0 - DAT_004534f8;
    _DAT_00436714 = DAT_00453514;
    _DAT_00436718 = DAT_00453518;
    _DAT_0043671c = DAT_0045351c;
    _DAT_00436720 = DAT_00453520;
    _DAT_0043672c = DAT_00453554;
    _DAT_00436724 = DAT_00453524;
    _DAT_00436748 = DAT_004534ec;
    _DAT_0043674c = DAT_004534f0;
    DAT_00436754 = DAT_004534d8;
    _DAT_00436738 = DAT_00453560;
    _DAT_0043673c = DAT_0045355c;
    DAT_004534dc = DAT_004366f8;
    DAT_004534e0 = DAT_004366f8;
    FUN_0040e9cc();
  }
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_0040ce20(void)

{
  int in_EAX;
  
  _DAT_00434082 = in_EAX + 1;
  _DAT_00434086 = 1 - in_EAX;
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_0040e9cc(void)

{
  undefined1 uVar1;
  bool bVar2;
  float fVar3;
  float fVar4;
  double dVar5;
  float fVar6;
  float fVar7;
  float fVar8;
  float fVar9;
  float fVar10;
  uint uVar11;
  int iVar12;
  float fVar13;
  uint uVar14;
  uint uVar15;
  uint uVar16;
  undefined2 uVar17;
  int iVar18;
  int iVar19;
  int iVar20;
  int iVar21;
  int iVar22;
  uint uVar23;
  
  DAT_004366c8 = 0x37f;
  DAT_004366ca = 0x7f;
  if (DAT_00436708 <= DAT_004366f8) {
    do {
      iVar22 = DAT_00436760;
      iVar12 = DAT_0043675c;
      fVar10 = _DAT_004366e4;
      DAT_0043679c = DAT_004366fc + _DAT_0043676c;
      DAT_00436798 = (uint)ROUND((float)DAT_004366fc * _DAT_00436728 + (float)_DAT_00436768);
      uVar14 = DAT_00436798;
      DAT_004367c4 = DAT_00436708 + _DAT_00436718;
      DAT_004367c0 = (uint)ROUND((float)DAT_00436708 * _DAT_0043672c + (float)_DAT_00436714);
      fVar13 = _DAT_00436700 / (_DAT_00436700 * _DAT_00436734 + _DAT_00436730);
      if (_DAT_00436758 == 0) {
        if (((int)DAT_00436798 < (int)DAT_0041cde8) && ((int)DAT_0041cde0 <= (int)DAT_004367c0)) {
LAB_0040eab7:
          if ((int)DAT_00436798 >> 8 != (int)DAT_004367c0 >> 8) {
            DAT_0043677c = (int)ROUND(fVar13 * (float)_DAT_00436740 + (float)_DAT_00436774);
            iVar18 = DAT_0043677c;
            DAT_00436780 = (uint)ROUND(fVar13 * (float)_DAT_00436744 + (float)_DAT_00436778);
            iVar19 = DAT_00436780;
            DAT_004367b0 = (int)ROUND(fVar13 * (float)DAT_00436750 + (float)_DAT_00436770);
            iVar20 = DAT_004367b0;
            fVar13 = _DAT_0043670c / (_DAT_0043670c * _DAT_0043673c + _DAT_00436738);
            if (_DAT_00436758 == 0) {
              _DAT_004367c8 = DAT_004367c0 - DAT_00436798;
            }
            else {
              _DAT_004367c8 = DAT_00436798 - DAT_004367c0;
            }
            DAT_00436784 = (int)ROUND(fVar13 * (float)_DAT_00436748 + (float)_DAT_00436720);
            DAT_00436788 = (int)ROUND(fVar13 * (float)_DAT_0043674c + (float)_DAT_00436724);
            DAT_004367b4 = (int)ROUND(fVar13 * (float)DAT_00436754 + (float)_DAT_0043671c);
            uVar15 = DAT_0043679c;
            if (_DAT_00436758 == 0) {
              DAT_004367b0 = DAT_004367b4;
              DAT_004367b4 = iVar20;
              DAT_00436798 = DAT_004367c0;
              DAT_004367c0 = uVar14;
              DAT_0043677c = DAT_00436784;
              DAT_00436780 = DAT_00436788;
              DAT_00436784 = iVar18;
              DAT_00436788 = iVar19;
              uVar15 = DAT_004367c4;
              DAT_004367c4 = DAT_0043679c;
            }
            DAT_0043679c = uVar15;
            _DAT_004367d4 = DAT_004367b4 - DAT_004367b0;
            _DAT_004367b8 = DAT_00436784 - DAT_0043677c;
            _DAT_004367bc = DAT_00436788 - DAT_00436780;
            _DAT_004367ac =
                 (DAT_00436798 - (DAT_004367c0 & 0xffffff00)) + -0x100 +
                 (0x100 - (DAT_004367c0 & 0xff) & 0x100);
            _DAT_004367cc = (float)DAT_004367b4 * (float)_DAT_004367c8 * (1.0 / (float)DAT_004367b0)
            ;
            _DAT_004367d0 = -(float)_DAT_004367d4 * (1.0 / (float)DAT_004367b0);
            if ((int)DAT_004367c0 < (int)DAT_0041cde0) {
              _DAT_004367ac =
                   _DAT_004367ac - ((DAT_0041cde0 & 0xffffff00) - (DAT_004367c0 & 0xffffff00));
              DAT_004367c0 = DAT_0041cde0;
            }
            fVar13 = (float)(int)_DAT_004367ac /
                     ((float)(int)_DAT_004367ac * _DAT_004367d0 + _DAT_004367cc);
            _DAT_004367ac = DAT_00436798 & 0xff;
            if ((int)DAT_0041cde8 <= (int)DAT_00436798) {
              _DAT_004367ac =
                   _DAT_004367ac + ((DAT_00436798 & 0xffffff00) - (DAT_0041cde8 & 0xffffff00));
              DAT_00436798 = DAT_0041cde8;
            }
            DAT_00436784 = (int)ROUND(fVar13 * (float)_DAT_004367b8 + (float)DAT_0043677c);
            DAT_00436788 = (int)ROUND(fVar13 * (float)_DAT_004367bc + (float)(int)DAT_00436780);
            DAT_004367b4 = (int)ROUND(fVar13 * (float)_DAT_004367d4 + (float)DAT_004367b0);
            fVar13 = (float)(int)_DAT_004367ac /
                     ((float)(int)_DAT_004367ac * _DAT_004367d0 + _DAT_004367cc);
            DAT_00436798 = (DAT_00436798 & 0xffffff00) - 0x100;
            DAT_004367a0 = ((int)DAT_00436798 >> 8) - ((int)DAT_004367c0 >> 8);
            DAT_0043677c = (int)ROUND(fVar13 * (float)_DAT_004367b8 + (float)DAT_0043677c);
            DAT_00436780 = (uint)ROUND(fVar13 * (float)_DAT_004367bc + (float)(int)DAT_00436780);
            DAT_004367b0 = (int)ROUND(fVar13 * (float)_DAT_004367d4 + (float)DAT_004367b0);
            fVar13 = DAT_004366bc;
            if (((((int)DAT_0041cde0 <= (int)DAT_00436798) &&
                 ((int)DAT_00436798 < (int)DAT_0041cde8)) &&
                (uVar14 = DAT_0043679c - 0x100, DAT_0041cde4 <= (int)uVar14)) &&
               ((int)uVar14 < DAT_0041cdec)) {
              uVar15 = DAT_0043677c << 0x10 | DAT_00436780;
              DAT_00436794 = DAT_0043675c;
              fVar3 = (float)DAT_004367b4 * (float)DAT_004367a0 * (1.0 / (float)DAT_004367b0);
              fVar4 = ((float)DAT_004367b0 - (float)DAT_004367b4) * (1.0 / (float)DAT_004367b0);
              DAT_00436790 = (uVar14 >> 8) * DAT_0041cdf0 + _DAT_0041cda4;
              DAT_0043678c = uVar15;
              DAT_0043679c = uVar14;
              _DAT_004367a4 = fVar3;
              _DAT_004367a8 = fVar4;
              if ((_DAT_00436764 & 1) == 0) {
                _DAT_004366f0 = (int)ROUND((float10)DAT_00436784 - (float10)DAT_0043677c);
                _DAT_004366f4 = (int)ROUND((float10)DAT_00436788 - (float10)(int)DAT_00436780);
                if (_DAT_004366f4 == 0) {
                  _DAT_004366f4 = 1;
                }
                dVar5 = (double)_DAT_004366f0 / (double)_DAT_004366f4;
                uVar14 = DAT_004367a0 + 1;
                uVar23 = DAT_00436790 + (DAT_00436798 >> 8) + 1;
                if ((int)uVar14 < 0x10) goto LAB_0040f317;
                fVar9 = (float)_DAT_004366f4;
                fVar7 = (float)dVar5;
                uVar16 = uVar23 & 3;
                fVar6 = (float)uVar16;
                fVar13 = (float)_DAT_004366c4;
                uVar14 = uVar14 - uVar16;
                _DAT_004366d4 = dVar5;
                if (uVar16 == 0) {
                  DAT_004366e8 = uVar14 >> 4;
                  DAT_004366ec = uVar14 & 0xf;
                  if (DAT_004366ec == 0) {
                    DAT_004366ec = 0x10;
                    DAT_004366e8 = DAT_004366e8 - 1;
                  }
                  if (0 < (int)DAT_004366e8) {
                    fVar8 = ((fVar6 + _DAT_004366e4) * fVar9) /
                            (fVar4 * (fVar6 + _DAT_004366e4) + fVar3);
                    goto LAB_0040f11f;
                  }
                }
                else {
                  fVar8 = fVar9 / (fVar4 * fVar6 + fVar3);
                  DAT_004366bc = (float)(int)ROUND(fVar8 * fVar7);
                  iVar22 = (int)DAT_004366bc;
                  DAT_004366bc = (float)(int)ROUND(fVar8);
                  fVar8 = ((fVar6 + _DAT_004366e4) * fVar9) /
                          (fVar4 * (fVar6 + _DAT_004366e4) + fVar3);
                  DAT_004366e8 = uVar14;
                  do {
                    uVar14 = uVar15 >> 8;
                    uVar11 = uVar15 >> 0x18;
                    uVar15 = uVar15 + (iVar22 << 0x10 | (uint)DAT_004366bc & 0xffff);
                    *(undefined1 *)(uVar23 - 1) =
                         *(undefined1 *)(iVar12 + (uint)CONCAT11((char)uVar14,(char)uVar11));
                    uVar23 = uVar23 - 1;
                    uVar16 = uVar16 - 1;
                  } while (uVar16 != 0);
                  uVar14 = DAT_004366e8 >> 4;
                  DAT_004366ec = DAT_004366e8 & 0xf;
                  if (DAT_004366ec == 0) {
                    DAT_004366ec = 0x10;
                    uVar14 = uVar14 - 1;
                  }
                  DAT_004366e8 = uVar14;
                  if (0 < (int)uVar14) {
LAB_0040f11f:
                    DAT_004366bc = (float)(int)ROUND(fVar8 * fVar7);
                    iVar22 = (int)DAT_004366bc;
                    DAT_004366bc = (float)(int)ROUND(fVar8);
                    iVar22 = iVar22 + DAT_0043677c;
                    uVar14 = (int)DAT_004366bc + DAT_00436780;
                    fVar10 = fVar6 + fVar10 + _DAT_004366e4;
                    do {
                      DAT_004366d0 = iVar22 << 0x10 | uVar14 & 0xffff;
                      uVar14 = (int)((uVar14 & 0xffff) - (uVar15 & 0xffff)) >> 4;
                      uVar16 = ((int)(iVar22 - (uVar15 >> 0x10)) >> 4) << 0x10 | uVar14 & 0xffff;
                      iVar22 = CONCAT22((short)(uVar15 >> 0x10),(short)uVar15 - (short)uVar14) +
                               uVar16;
                      iVar18 = iVar22 + uVar16;
                      iVar19 = iVar18 + uVar16;
                      uVar17 = CONCAT11(*(undefined1 *)
                                         (iVar12 + (uint)CONCAT11((char)((uint)iVar22 >> 8),
                                                                  (char)(uVar15 >> 0x18))),
                                        *(undefined1 *)
                                         (iVar12 + (uint)CONCAT11((char)((uint)iVar18 >> 8),
                                                                  (char)((uint)iVar22 >> 0x18))));
                      fVar6 = (fVar10 * fVar9) / (fVar10 * fVar4 + fVar3);
                      iVar22 = iVar19 + uVar16;
                      iVar20 = iVar22 + uVar16;
                      *(uint *)(uVar23 - 4) =
                           CONCAT31(CONCAT21(uVar17,*(undefined1 *)
                                                     (iVar12 + (uint)CONCAT11((char)((uint)iVar19 >>
                                                                                    8),(char)((uint)
                                                  iVar18 >> 0x18)))),
                                    *(undefined1 *)
                                     (iVar12 + (uint)CONCAT11((char)((uint)iVar22 >> 8),
                                                              (char)((uint)iVar19 >> 0x18))));
                      iVar18 = iVar20 + uVar16;
                      iVar19 = iVar18 + uVar16;
                      iVar21 = iVar19 + uVar16;
                      uVar17 = (undefined2)
                               CONCAT31(CONCAT21(uVar17,*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar20 >> 8),(char)((uint)iVar22 >> 0x18)))),
                                        *(undefined1 *)
                                         (iVar12 + (uint)CONCAT11((char)((uint)iVar18 >> 8),
                                                                  (char)((uint)iVar20 >> 0x18))));
                      iVar22 = iVar21 + uVar16;
                      *(uint *)(uVar23 - 8) =
                           CONCAT31(CONCAT21(uVar17,*(undefined1 *)
                                                     (iVar12 + (uint)CONCAT11((char)((uint)iVar19 >>
                                                                                    8),(char)((uint)
                                                  iVar18 >> 0x18)))),
                                    *(undefined1 *)
                                     (iVar12 + (uint)CONCAT11((char)((uint)iVar21 >> 8),
                                                              (char)((uint)iVar19 >> 0x18))));
                      iVar18 = iVar22 + uVar16;
                      DAT_004366bc = fVar6 * fVar7 + fVar13;
                      fVar6 = fVar6 + fVar13;
                      iVar19 = iVar18 + uVar16;
                      iVar20 = iVar19 + uVar16;
                      fVar10 = fVar10 + _DAT_004366e4;
                      uVar17 = (undefined2)
                               CONCAT31(CONCAT21(uVar17,*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar22 >> 8),(char)((uint)iVar21 >> 0x18)))),
                                        *(undefined1 *)
                                         (iVar12 + (uint)CONCAT11((char)((uint)iVar18 >> 8),
                                                                  (char)((uint)iVar22 >> 0x18))));
                      iVar21 = iVar20 + uVar16;
                      *(uint *)(uVar23 - 0xc) =
                           CONCAT31(CONCAT21(uVar17,*(undefined1 *)
                                                     (iVar12 + (uint)CONCAT11((char)((uint)iVar19 >>
                                                                                    8),(char)((uint)
                                                  iVar18 >> 0x18)))),
                                    *(undefined1 *)
                                     (iVar12 + (uint)CONCAT11((char)((uint)iVar20 >> 8),
                                                              (char)((uint)iVar19 >> 0x18))));
                      uVar15 = DAT_004366d0;
                      iVar18 = iVar21 + uVar16;
                      iVar19 = iVar18 + uVar16;
                      iVar22 = (int)DAT_004366bc + DAT_0043677c;
                      uVar14 = (int)fVar6 + DAT_00436780;
                      DAT_004366bc = fVar6;
                      *(uint *)(uVar23 - 0x10) =
                           CONCAT31(CONCAT21((short)CONCAT31(CONCAT21(uVar17,*(undefined1 *)
                                                                              (iVar12 + (uint)
                                                  CONCAT11((char)((uint)iVar21 >> 8),
                                                           (char)((uint)iVar20 >> 0x18)))),
                                                  *(undefined1 *)
                                                   (iVar12 + (uint)CONCAT11((char)((uint)iVar18 >> 8
                                                                                  ),(char)((uint)
                                                  iVar21 >> 0x18)))),
                                             *(undefined1 *)
                                              (iVar12 + (uint)CONCAT11((char)((uint)iVar19 >> 8),
                                                                       (char)((uint)iVar18 >> 0x18))
                                              )),
                                    *(undefined1 *)
                                     (iVar12 + (uint)CONCAT11((char)(iVar19 + uVar16 >> 8),
                                                              (char)((uint)iVar19 >> 0x18))));
                      uVar23 = uVar23 - 0x10;
                      DAT_004366e8 = DAT_004366e8 - 1;
                    } while (DAT_004366e8 != 0);
                    fVar13 = DAT_004366bc;
                    if (0x10 < (int)DAT_004366ec) goto LAB_0040eb90;
                  }
                }
                _DAT_004366f0 = _DAT_004366f0 - ((uVar15 >> 0x10) - (DAT_0043678c >> 0x10));
                _DAT_004366f4 = _DAT_004366f4 - ((uVar15 & 0xffff) - (DAT_0043678c & 0xffff));
                uVar14 = DAT_004366ec;
                DAT_0043678c = uVar15;
LAB_0040f317:
                DAT_004366bc = (float)(int)ROUND((float)_DAT_004366f4 *
                                                 (float)(&DAT_004367d8)[uVar14]);
                    // WARNING: Could not recover jumptable at 0x0040f384. Too many branches
                    // WARNING: Treating indirect jump as call
                (*(code *)(&DAT_0040f386 + (&DAT_00436828)[uVar14]))();
                return;
              }
              _DAT_004366f0 = (int)ROUND((float10)DAT_00436784 - (float10)DAT_0043677c);
              _DAT_004366f4 = (int)ROUND((float10)DAT_00436788 - (float10)(int)DAT_00436780);
              if (_DAT_004366f4 == 0) {
                _DAT_004366f4 = 1;
              }
              dVar5 = (double)_DAT_004366f0 / (double)_DAT_004366f4;
              uVar14 = DAT_004367a0 + 1;
              uVar23 = DAT_00436790 + (DAT_00436798 >> 8) + 1;
              if (0xf < (int)uVar14) {
                fVar6 = (float)_DAT_004366f4;
                fVar9 = (float)dVar5;
                uVar16 = uVar23 & 3;
                fVar13 = (float)uVar16;
                fVar7 = (float)_DAT_004366c4;
                uVar14 = uVar14 - uVar16;
                _DAT_004366d4 = dVar5;
                if (uVar16 == 0) {
                  DAT_004366e8 = uVar14 >> 4;
                  DAT_004366ec = uVar14 & 0xf;
                  if (DAT_004366ec == 0) {
                    DAT_004366ec = 0x10;
                    DAT_004366e8 = DAT_004366e8 - 1;
                  }
                  if (0 < (int)DAT_004366e8) {
                    fVar8 = ((fVar13 + _DAT_004366e4) * fVar6) /
                            (fVar4 * (fVar13 + _DAT_004366e4) + fVar3);
                    goto LAB_0040f628;
                  }
                }
                else {
                  fVar8 = fVar6 / (fVar4 * fVar13 + fVar3);
                  DAT_004366bc = (float)(int)ROUND(fVar8 * fVar9);
                  iVar18 = (int)DAT_004366bc;
                  DAT_004366bc = (float)(int)ROUND(fVar8);
                  fVar8 = ((fVar13 + _DAT_004366e4) * fVar6) /
                          (fVar4 * (fVar13 + _DAT_004366e4) + fVar3);
                  DAT_004366e8 = uVar14;
                  do {
                    uVar14 = uVar15 >> 0x18;
                    uVar11 = uVar15 >> 8;
                    uVar15 = uVar15 + (iVar18 << 0x10 | (uint)DAT_004366bc & 0xffff);
                    *(undefined1 *)(uVar23 - 1) =
                         *(undefined1 *)
                          (iVar22 + (uint)CONCAT11(*(undefined1 *)
                                                    (iVar12 + (uint)CONCAT11((char)uVar11,
                                                                             (char)uVar14)),
                                                   *(undefined1 *)(uVar23 - 1)));
                    uVar23 = uVar23 - 1;
                    uVar16 = uVar16 - 1;
                  } while (uVar16 != 0);
                  uVar14 = DAT_004366e8 >> 4;
                  DAT_004366ec = DAT_004366e8 & 0xf;
                  if (DAT_004366ec == 0) {
                    DAT_004366ec = 0x10;
                    uVar14 = uVar14 - 1;
                  }
                  DAT_004366e8 = uVar14;
                  if (0 < (int)uVar14) {
LAB_0040f628:
                    DAT_004366bc = (float)(int)ROUND(fVar8 * fVar9);
                    iVar22 = (int)DAT_004366bc;
                    DAT_004366bc = (float)(int)ROUND(fVar8);
                    iVar22 = iVar22 + DAT_0043677c;
                    uVar14 = (int)DAT_004366bc + DAT_00436780;
                    fVar10 = fVar13 + fVar10 + _DAT_004366e4;
                    do {
                      iVar18 = DAT_00436760;
                      DAT_004366d0 = iVar22 << 0x10 | uVar14 & 0xffff;
                      uVar14 = (int)((uVar14 & 0xffff) - (uVar15 & 0xffff)) >> 4;
                      uVar16 = ((int)(iVar22 - (uVar15 >> 0x10)) >> 4) << 0x10 | uVar14 & 0xffff;
                      iVar22 = CONCAT22((short)(uVar15 >> 0x10),(short)uVar15 - (short)uVar14) +
                               uVar16;
                      iVar19 = iVar22 + uVar16;
                      uVar1 = *(undefined1 *)
                               (DAT_00436760 +
                               (uint)CONCAT11(*(undefined1 *)
                                               (iVar12 + (uint)CONCAT11((char)((uint)iVar19 >> 8),
                                                                        (char)((uint)iVar22 >> 0x18)
                                                                       )),
                                              *(undefined1 *)(uVar23 - 2)));
                      *(undefined1 *)(uVar23 - 1) =
                           *(undefined1 *)
                            (DAT_00436760 +
                            (uint)CONCAT11(*(undefined1 *)
                                            (iVar12 + (uint)CONCAT11((char)((uint)iVar22 >> 8),
                                                                     (char)(uVar15 >> 0x18))),
                                           *(undefined1 *)(uVar23 - 1)));
                      *(undefined1 *)(uVar23 - 2) = uVar1;
                      iVar22 = iVar19 + uVar16;
                      iVar20 = iVar22 + uVar16;
                      uVar1 = *(undefined1 *)
                               (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar20 >> 8),(char)((uint)iVar22 >> 0x18))),
                                                  *(undefined1 *)(uVar23 - 4)));
                      *(undefined1 *)(uVar23 - 3) =
                           *(undefined1 *)
                            (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                      (iVar12 + (uint)CONCAT11((char)((uint)iVar22
                                                                                     >> 8),
                                                                               (char)((uint)iVar19
                                                                                     >> 0x18))),
                                                     *(undefined1 *)(uVar23 - 3)));
                      *(undefined1 *)(uVar23 - 4) = uVar1;
                      iVar22 = iVar20 + uVar16;
                      iVar19 = iVar22 + uVar16;
                      fVar13 = (fVar10 * fVar6) / (fVar10 * fVar4 + fVar3);
                      uVar1 = *(undefined1 *)
                               (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar19 >> 8),(char)((uint)iVar22 >> 0x18))),
                                                  *(undefined1 *)(uVar23 - 6)));
                      *(undefined1 *)(uVar23 - 5) =
                           *(undefined1 *)
                            (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                      (iVar12 + (uint)CONCAT11((char)((uint)iVar22
                                                                                     >> 8),
                                                                               (char)((uint)iVar20
                                                                                     >> 0x18))),
                                                     *(undefined1 *)(uVar23 - 5)));
                      *(undefined1 *)(uVar23 - 6) = uVar1;
                      iVar22 = iVar19 + uVar16;
                      iVar20 = iVar22 + uVar16;
                      uVar1 = *(undefined1 *)
                               (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar20 >> 8),(char)((uint)iVar22 >> 0x18))),
                                                  *(undefined1 *)(uVar23 - 8)));
                      *(undefined1 *)(uVar23 - 7) =
                           *(undefined1 *)
                            (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                      (iVar12 + (uint)CONCAT11((char)((uint)iVar22
                                                                                     >> 8),
                                                                               (char)((uint)iVar19
                                                                                     >> 0x18))),
                                                     *(undefined1 *)(uVar23 - 7)));
                      *(undefined1 *)(uVar23 - 8) = uVar1;
                      iVar22 = iVar20 + uVar16;
                      iVar19 = iVar22 + uVar16;
                      uVar1 = *(undefined1 *)
                               (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar19 >> 8),(char)((uint)iVar22 >> 0x18))),
                                                  *(undefined1 *)(uVar23 - 10)));
                      *(undefined1 *)(uVar23 - 9) =
                           *(undefined1 *)
                            (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                      (iVar12 + (uint)CONCAT11((char)((uint)iVar22
                                                                                     >> 8),
                                                                               (char)((uint)iVar20
                                                                                     >> 0x18))),
                                                     *(undefined1 *)(uVar23 - 9)));
                      *(undefined1 *)(uVar23 - 10) = uVar1;
                      iVar22 = iVar19 + uVar16;
                      iVar20 = iVar22 + uVar16;
                      uVar1 = *(undefined1 *)
                               (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar20 >> 8),(char)((uint)iVar22 >> 0x18))),
                                                  *(undefined1 *)(uVar23 - 0xc)));
                      *(undefined1 *)(uVar23 - 0xb) =
                           *(undefined1 *)
                            (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                      (iVar12 + (uint)CONCAT11((char)((uint)iVar22
                                                                                     >> 8),
                                                                               (char)((uint)iVar19
                                                                                     >> 0x18))),
                                                     *(undefined1 *)(uVar23 - 0xb)));
                      *(undefined1 *)(uVar23 - 0xc) = uVar1;
                      iVar22 = iVar20 + uVar16;
                      iVar19 = iVar22 + uVar16;
                      DAT_004366bc = fVar13 * fVar9 + fVar7;
                      fVar13 = fVar13 + fVar7;
                      uVar1 = *(undefined1 *)
                               (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)((uint)
                                                  iVar19 >> 8),(char)((uint)iVar22 >> 0x18))),
                                                  *(undefined1 *)(uVar23 - 0xe)));
                      *(undefined1 *)(uVar23 - 0xd) =
                           *(undefined1 *)
                            (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                      (iVar12 + (uint)CONCAT11((char)((uint)iVar22
                                                                                     >> 8),
                                                                               (char)((uint)iVar20
                                                                                     >> 0x18))),
                                                     *(undefined1 *)(uVar23 - 0xd)));
                      *(undefined1 *)(uVar23 - 0xe) = uVar1;
                      uVar15 = DAT_004366d0;
                      fVar10 = fVar10 + _DAT_004366e4;
                      iVar22 = iVar19 + uVar16;
                      uVar1 = *(undefined1 *)
                               (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                         (iVar12 + (uint)CONCAT11((char)(iVar22 + 
                                                  uVar16 >> 8),(char)((uint)iVar22 >> 0x18))),
                                                  *(undefined1 *)(uVar23 - 0x10)));
                      *(undefined1 *)(uVar23 - 0xf) =
                           *(undefined1 *)
                            (iVar18 + (uint)CONCAT11(*(undefined1 *)
                                                      (iVar12 + (uint)CONCAT11((char)((uint)iVar22
                                                                                     >> 8),
                                                                               (char)((uint)iVar19
                                                                                     >> 0x18))),
                                                     *(undefined1 *)(uVar23 - 0xf)));
                      *(undefined1 *)(uVar23 - 0x10) = uVar1;
                      iVar22 = (int)DAT_004366bc + DAT_0043677c;
                      uVar23 = uVar23 - 0x10;
                      uVar14 = (int)fVar13 + DAT_00436780;
                      DAT_004366e8 = DAT_004366e8 - 1;
                    } while (DAT_004366e8 != 0);
                    if (0x10 < (int)DAT_004366ec) goto LAB_0040eb90;
                  }
                }
                _DAT_004366f0 = _DAT_004366f0 - ((uVar15 >> 0x10) - (DAT_0043678c >> 0x10));
                _DAT_004366f4 = _DAT_004366f4 - ((uVar15 & 0xffff) - (DAT_0043678c & 0xffff));
                uVar14 = DAT_004366ec;
                DAT_0043678c = uVar15;
              }
              iVar18 = DAT_00436794;
              iVar22 = DAT_00436760;
              iVar19 = (uVar23 - uVar14) + -1;
              DAT_004366bc = (float)(int)ROUND((float)_DAT_004366f0 * (float)(&DAT_004367d8)[uVar14]
                                              );
              iVar12 = (int)DAT_004366bc;
              DAT_004366bc = (float)(int)ROUND((float)_DAT_004366f4 * (float)(&DAT_004367d8)[uVar14]
                                              );
              uVar23 = (uint)DAT_004366bc & 0xffff;
              uVar15 = DAT_0043678c;
              do {
                uVar16 = uVar15 >> 0x18;
                uVar11 = uVar15 >> 8;
                uVar15 = uVar15 + (iVar12 << 0x10 | uVar23);
                *(undefined1 *)(iVar19 + uVar14) =
                     *(undefined1 *)
                      (iVar22 + (uint)CONCAT11(*(undefined1 *)
                                                (iVar18 + (uint)CONCAT11((char)uVar11,(char)uVar16))
                                               ,*(undefined1 *)(iVar19 + uVar14)));
                uVar16 = uVar14 - 1;
                bVar2 = 0 < (int)uVar14;
                uVar14 = uVar16;
                fVar13 = DAT_004366bc;
              } while (uVar16 != 0 && bVar2);
            }
          }
        }
      }
      else if (((int)DAT_0041cde0 <= (int)DAT_00436798) && ((int)DAT_004367c0 < (int)DAT_0041cde8))
      goto LAB_0040eab7;
LAB_0040eb90:
      DAT_004366bc = fVar13;
      DAT_004366fc = DAT_004366fc + 0x100;
      DAT_00436708 = DAT_00436708 + 0x100;
      _DAT_00436700 = (float)DAT_004366fc * _DAT_00436704;
      _DAT_0043670c = (float)DAT_00436708 * _DAT_00436710;
    } while (DAT_00436708 <= DAT_004366f8);
  }
  return;
}



// Library Function - Single Match
//  __ftol
// 
// Library: Visual Studio

longlong __ftol(void)

{
  float10 in_ST0;
  
  return (longlong)ROUND(in_ST0);
}



// Library Function - Single Match
//  __fpmath
// 
// Library: Visual Studio 1998 Release

void __cdecl __fpmath(int param_1)

{
  FUN_00410e50();
  DAT_0043aa48 = __ms_p5_mp_test_fdiv();
  __setdefaultprecision();
  return;
}



void FUN_00410e50(void)

{
  PTR_FUN_0043aac4 = &LAB_00411b10;
  PTR_FUN_0043aac8 = &LAB_00411b90;
  PTR_FUN_0043aacc = &LAB_00411aa0;
  PTR_FUN_0043aad0 = &LAB_00411b70;
  PTR_FUN_0043aac0 = &LAB_00411f30;
  PTR_FUN_0043aad4 = &LAB_00411f30;
  return;
}



// Library Function - Single Match
//  __cinit
// 
// Library: Visual Studio 1998 Release

int __cdecl __cinit(int param_1)

{
  int iVar1;
  
  if (PTR___fpmath_0043aa4c != (undefined *)0x0) {
    (*(code *)PTR___fpmath_0043aa4c)();
  }
  __initterm((undefined4 *)&DAT_0041c008,(undefined4 *)&DAT_0041c010);
  iVar1 = __initterm((undefined4 *)&DAT_0041c000,(undefined4 *)&DAT_0041c004);
  return iVar1;
}



// Library Function - Single Match
//  _exit
// 
// Library: Visual Studio 1998 Release

void __cdecl _exit(int _Code)

{
  doexit(_Code,0,0);
  return;
}



// Library Function - Single Match
//  __exit
// 
// Library: Visual Studio 1998 Release

void __cdecl __exit(UINT param_1)

{
  doexit(param_1,1,0);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  _doexit
// 
// Library: Visual Studio 1998 Release

void __cdecl doexit(UINT param_1,int param_2,int param_3)

{
  HANDLE hProcess;
  undefined4 *puVar1;
  UINT uExitCode;
  
  if (DAT_0043aaa0 == 1) {
    uExitCode = param_1;
    hProcess = GetCurrentProcess();
    TerminateProcess(hProcess,uExitCode);
  }
  _DAT_0043aa9c = 1;
  DAT_0043aa98 = (undefined1)param_3;
  if (param_2 == 0) {
    if ((DAT_0045468c != (undefined4 *)0x0) &&
       (puVar1 = (undefined4 *)(DAT_00454688 + -4), DAT_0045468c <= puVar1)) {
      do {
        if ((code *)*puVar1 != (code *)0x0) {
          (*(code *)*puVar1)();
        }
        puVar1 = puVar1 + -1;
      } while (DAT_0045468c <= puVar1);
    }
    __initterm((undefined4 *)&DAT_0041c014,(undefined4 *)&DAT_0041c01c);
  }
  __initterm((undefined4 *)&DAT_0041c020,(undefined4 *)&DAT_0041c024);
  if (param_3 == 0) {
    DAT_0043aaa0 = 1;
                    // WARNING: Subroutine does not return
    ExitProcess(param_1);
  }
  return;
}



// Library Function - Single Match
//  __initterm
// 
// Library: Visual Studio 1998 Release

void __cdecl __initterm(undefined4 *param_1,undefined4 *param_2)

{
  if (param_1 < param_2) {
    do {
      if ((code *)*param_1 != (code *)0x0) {
        (*(code *)*param_1)();
      }
      param_1 = param_1 + 1;
    } while (param_1 < param_2);
  }
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void entry(void)

{
  byte bVar1;
  DWORD DVar2;
  int iVar3;
  HMODULE pHVar4;
  byte *pbVar5;
  int unaff_EDI;
  undefined4 *unaff_FS_OFFSET;
  _STARTUPINFOA local_74;
  undefined1 *local_1c;
  undefined4 uStack_14;
  undefined1 *puStack_10;
  undefined *puStack_c;
  undefined4 local_8;
  
  uStack_14 = *unaff_FS_OFFSET;
  local_8 = 0xffffffff;
  puStack_c = &DAT_0041b2d0;
  puStack_10 = &LAB_00412ca4;
  *unaff_FS_OFFSET = &uStack_14;
  local_1c = &stack0xffffff80;
  DVar2 = GetVersion();
  _DAT_0043aa70 = DVar2 >> 8 & 0xff;
  DAT_0043aa64 = DVar2 >> 0x10;
  _DAT_0043aa6c = DVar2 & 0xff;
  _DAT_0043aa68 = _DAT_0043aa6c * 0x100 + _DAT_0043aa70;
  iVar3 = __heap_init();
  if (iVar3 == 0) {
    __amsg_exit(0x1c);
  }
  local_8 = 0;
  __ioinit();
  ___initmbctable();
  DAT_00454684 = (byte *)GetCommandLineA();
  DAT_0043aaa4 = ___crtGetEnvironmentStringsA();
  if ((DAT_0043aaa4 == (LPVOID)0x0) || (DAT_00454684 == (byte *)0x0)) {
                    // WARNING: Subroutine does not return
    _exit(-1);
  }
  __setargv();
  __setenvp();
  __cinit(unaff_EDI);
  bVar1 = *DAT_00454684;
  pbVar5 = DAT_00454684;
  if (bVar1 == 0x22) {
    pbVar5 = DAT_00454684 + 1;
    if (*pbVar5 != 0x22) {
      do {
        if (*pbVar5 == 0) break;
        iVar3 = __ismbblead((uint)*pbVar5);
        if (iVar3 != 0) {
          pbVar5 = pbVar5 + 1;
        }
        pbVar5 = pbVar5 + 1;
      } while (*pbVar5 != 0x22);
      if (*pbVar5 != 0x22) goto LAB_004110d5;
    }
    pbVar5 = pbVar5 + 1;
  }
  else {
    while (0x20 < bVar1) {
      bVar1 = pbVar5[1];
      pbVar5 = pbVar5 + 1;
    }
  }
LAB_004110d5:
  bVar1 = *pbVar5;
  while ((bVar1 != 0 && (*pbVar5 < 0x21))) {
    pbVar5 = pbVar5 + 1;
    bVar1 = *pbVar5;
  }
  local_74.dwFlags = 0;
  GetStartupInfoA(&local_74);
  pHVar4 = GetModuleHandleA((LPCSTR)0x0);
  iVar3 = FUN_00403140(pHVar4);
                    // WARNING: Subroutine does not return
  _exit(iVar3);
}



// Library Function - Single Match
//  __amsg_exit
// 
// Library: Visual Studio 1998 Release

void __cdecl __amsg_exit(int param_1)

{
  if (DAT_0043aab0 == 1) {
    __FF_MSGBANNER();
  }
  __NMSG_WRITE(param_1);
  (*(code *)PTR___exit_0043aaac)(0xff);
  return;
}



// Library Function - Single Match
//  _malloc
// 
// Library: Visual Studio 1998 Release

void * __cdecl _malloc(size_t _Size)

{
  void *pvVar1;
  
  pvVar1 = __nh_malloc(_Size,DAT_0043ae4c);
  return pvVar1;
}



// Library Function - Single Match
//  __nh_malloc
// 
// Library: Visual Studio 1998 Release

void * __cdecl __nh_malloc(size_t _Size,int _NhFlag)

{
  void *pvVar1;
  int iVar2;
  
  if (0xffffffe0 < _Size) {
    return (void *)0x0;
  }
  if (_Size == 0) {
    _Size = 1;
  }
  do {
    pvVar1 = (void *)0x0;
    if (_Size < 0xffffffe1) {
      pvVar1 = __heap_alloc(_Size);
    }
    if (pvVar1 != (void *)0x0) {
      return pvVar1;
    }
    if (_NhFlag == 0) {
      return (void *)0x0;
    }
    iVar2 = __callnewh(_Size);
  } while (iVar2 != 0);
  return (void *)0x0;
}



// Library Function - Single Match
//  __heap_alloc
// 
// Library: Visual Studio 1998 Release

void * __cdecl __heap_alloc(size_t _Size)

{
  undefined *puVar1;
  LPVOID pvVar2;
  uint dwBytes;
  
  dwBytes = _Size + 0xf & 0xfffffff0;
  if ((dwBytes <= DAT_0043b66c) &&
     (puVar1 = ___sbh_alloc_block(_Size + 0xf >> 4), puVar1 != (undefined *)0x0)) {
    return puVar1;
  }
  pvVar2 = HeapAlloc(DAT_00454574,0,dwBytes);
  return pvVar2;
}



// Library Function - Single Match
//  _free
// 
// Library: Visual Studio 1998 Release

void __cdecl _free(void *_Memory)

{
  char *pcVar1;
  uint local_8;
  int local_4;
  
  if (_Memory != (void *)0x0) {
    pcVar1 = (char *)___sbh_find_block((undefined *)_Memory,&local_4,&local_8);
    if (pcVar1 != (char *)0x0) {
      ___sbh_free_block(local_4,local_8,pcVar1);
      return;
    }
    HeapFree(DAT_00454574,0,_Memory);
  }
  return;
}



// Library Function - Single Match
//  _fclose
// 
// Library: Visual Studio 1998 Release

int __cdecl _fclose(FILE *_File)

{
  int iVar1;
  int iVar2;
  
  iVar2 = -1;
  if ((_File->_flag & 0x40U) != 0) {
    _File->_flag = 0;
    return -1;
  }
  if ((_File->_flag & 0x83U) != 0) {
    iVar2 = __flush(_File);
    __freebuf(_File);
    iVar1 = __close(_File->_file);
    if (iVar1 < 0) {
      iVar2 = -1;
    }
    else if (_File->_tmpfname != (char *)0x0) {
      _free(_File->_tmpfname);
      _File->_tmpfname = (char *)0x0;
    }
  }
  _File->_flag = 0;
  return iVar2;
}



// Library Function - Single Match
//  _fread
// 
// Library: Visual Studio 1998 Release

size_t __cdecl _fread(void *_DstBuf,size_t _ElementSize,size_t _Count,FILE *_File)

{
  uint uVar1;
  int iVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  char *pcVar6;
  char *pcVar7;
  uint local_4;
  
  uVar1 = _Count * _ElementSize;
  if (uVar1 == 0) {
    return 0;
  }
  uVar5 = uVar1;
  if ((_File->_flag & 0x10cU) == 0) {
    local_4 = 0x1000;
  }
  else {
    local_4 = _File->_bufsiz;
  }
  while( true ) {
    while( true ) {
      while( true ) {
        if (uVar5 == 0) {
          return _Count;
        }
        if (((_File->_flag & 0x10cU) == 0) || (uVar3 = _File->_cnt, uVar3 == 0)) break;
        uVar4 = uVar5;
        if (uVar3 <= uVar5) {
          uVar4 = uVar3;
        }
        uVar5 = uVar5 - uVar4;
        pcVar6 = _File->_ptr;
        pcVar7 = (char *)_DstBuf;
        for (uVar3 = uVar4 >> 2; uVar3 != 0; uVar3 = uVar3 - 1) {
          *(undefined4 *)pcVar7 = *(undefined4 *)pcVar6;
          pcVar6 = pcVar6 + 4;
          pcVar7 = pcVar7 + 4;
        }
        for (uVar3 = uVar4 & 3; uVar3 != 0; uVar3 = uVar3 - 1) {
          *pcVar7 = *pcVar6;
          pcVar6 = pcVar6 + 1;
          pcVar7 = pcVar7 + 1;
        }
        _File->_cnt = _File->_cnt - uVar4;
        _File->_ptr = _File->_ptr + uVar4;
        _DstBuf = (char *)((int)_DstBuf + uVar4);
      }
      if (local_4 <= uVar5) break;
      iVar2 = __filbuf(_File);
      if (iVar2 == -1) {
        return (uVar1 - uVar5) / _ElementSize;
      }
      uVar5 = uVar5 - 1;
      *(char *)_DstBuf = (char)iVar2;
      local_4 = _File->_bufsiz;
      _DstBuf = (char *)((int)_DstBuf + 1);
    }
    uVar3 = uVar5;
    if (local_4 != 0) {
      uVar3 = uVar5 - uVar5 % local_4;
    }
    iVar2 = __read(_File->_file,_DstBuf,uVar3);
    if (iVar2 == 0) break;
    if (iVar2 == -1) {
      _File->_flag = _File->_flag | 0x20;
      return (uVar1 - uVar5) / _ElementSize;
    }
    uVar5 = uVar5 - iVar2;
    _DstBuf = (char *)((int)_DstBuf + iVar2);
  }
  _File->_flag = _File->_flag | 0x10;
  return (uVar1 - uVar5) / _ElementSize;
}



// Library Function - Single Match
//  _fsetpos
// 
// Library: Visual Studio 1998 Release

int __cdecl _fsetpos(FILE *_File,fpos_t *_Pos)

{
  int iVar1;
  int unaff_retaddr;
  
  iVar1 = __fseeki64(_File,(ulonglong)*(uint *)((int)_Pos + 4),unaff_retaddr);
  return iVar1;
}



// Library Function - Single Match
//  __fsopen
// 
// Library: Visual Studio 1998 Release

FILE * __cdecl __fsopen(char *_Filename,char *_Mode,int _ShFlag)

{
  FILE *pFVar1;
  
  pFVar1 = __getstream();
  if (pFVar1 == (FILE *)0x0) {
    return (FILE *)0x0;
  }
  pFVar1 = __openfile(_Filename,_Mode,_ShFlag,pFVar1);
  return pFVar1;
}



void __cdecl FUN_004114b0(char *param_1,char *param_2)

{
  __fsopen(param_1,param_2,0x40);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  _fseek
// 
// Library: Visual Studio 1998 Release

int __cdecl _fseek(FILE *_File,long _Offset,int _Origin)

{
  uint uVar1;
  long lVar2;
  
  if (((_File->_flag & 0x83U) != 0) && (((_Origin == 0 || (_Origin == 1)) || (_Origin == 2)))) {
    _File->_flag = _File->_flag & 0xffffffef;
    if (_Origin == 1) {
      _Origin = 0;
      lVar2 = _ftell(_File);
      _Offset = _Offset + lVar2;
    }
    __flush(_File);
    uVar1 = _File->_flag;
    if ((uVar1 & 0x80) == 0) {
      if ((((uVar1 & 1) != 0) && ((uVar1 & 8) != 0)) && ((uVar1 & 0x400) == 0)) {
        _File->_bufsiz = 0x200;
      }
    }
    else {
      _File->_flag = uVar1 & 0xfffffffc;
    }
    lVar2 = __lseek(_File->_file,_Offset,_Origin);
    return -(uint)(lVar2 == -1);
  }
  _DAT_0043aa58 = 0x16;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  _ftell
// 
// Library: Visual Studio 1998 Release

long __cdecl _ftell(FILE *_File)

{
  uint _FileHandle;
  byte bVar1;
  long _Offset;
  int iVar2;
  int *piVar3;
  long lVar4;
  char *pcVar5;
  char *pcVar6;
  int iVar7;
  uint uVar8;
  
  _FileHandle = _File->_file;
  if (_File->_cnt < 0) {
    _File->_cnt = 0;
  }
  _Offset = __lseek(_FileHandle,0,1);
  if (_Offset < 0) {
    return -1;
  }
  uVar8 = _File->_flag;
  if ((uVar8 & 0x108) == 0) {
    return _Offset - _File->_cnt;
  }
  pcVar6 = _File->_base;
  iVar2 = (int)_File->_ptr - (int)pcVar6;
  iVar7 = iVar2;
  if ((uVar8 & 3) == 0) {
    if ((uVar8 & 0x80) == 0) {
      _DAT_0043aa58 = 0x16;
      return -1;
    }
  }
  else if ((*(byte *)(*(int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7) >> 3)) + 4 +
                     (_FileHandle & 0x1f) * 8) & 0x80) != 0) {
    for (; pcVar6 < _File->_ptr; pcVar6 = pcVar6 + 1) {
      if (*pcVar6 == '\n') {
        iVar7 = iVar7 + 1;
      }
    }
  }
  if (_Offset != 0) {
    if ((uVar8 & 1) != 0) {
      if (_File->_cnt == 0) {
        return _Offset;
      }
      uVar8 = iVar2 + _File->_cnt;
      piVar3 = (int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7) >> 3));
      iVar2 = (_FileHandle & 0x1f) * 8;
      if ((*(byte *)(*piVar3 + 4 + iVar2) & 0x80) != 0) {
        lVar4 = __lseek(_FileHandle,0,2);
        if (lVar4 == _Offset) {
          pcVar5 = _File->_base;
          pcVar6 = pcVar5 + uVar8;
          for (; pcVar5 < pcVar6; pcVar5 = pcVar5 + 1) {
            if (*pcVar5 == '\n') {
              uVar8 = uVar8 + 1;
            }
          }
          bVar1 = *(byte *)((int)&_File->_flag + 1) & 0x20;
        }
        else {
          __lseek(_FileHandle,_Offset,0);
          if (((0x200 < uVar8) || ((_File->_flag & 8U) == 0)) ||
             (uVar8 = 0x200, (_File->_flag & 0x400U) != 0)) {
            uVar8 = _File->_bufsiz;
          }
          bVar1 = *(byte *)(*piVar3 + 4 + iVar2) & 4;
        }
        if (bVar1 != 0) {
          uVar8 = uVar8 + 1;
        }
      }
      _Offset = _Offset - uVar8;
    }
    return iVar7 + _Offset;
  }
  return iVar7;
}



// Library Function - Single Match
//  _printf
// 
// Library: Visual Studio 1998 Release

int __cdecl _printf(char *_Format,...)

{
  int _Flag;
  int iVar1;
  
  _Flag = __stbuf((FILE *)&DAT_0043b808);
  iVar1 = __output((FILE *)&DAT_0043b808,(byte *)_Format,(undefined4 *)&stack0x00000008);
  __ftbuf(_Flag,(FILE *)&DAT_0043b808);
  return iVar1;
}



// Library Function - Single Match
//  _calloc
// 
// Library: Visual Studio 1998 Release

void * __cdecl _calloc(size_t _Count,size_t _Size)

{
  int iVar1;
  uint uVar2;
  undefined4 *puVar3;
  uint dwBytes;
  undefined4 *puVar4;
  
  dwBytes = _Size * _Count;
  if (dwBytes < 0xffffffe1) {
    if (dwBytes == 0) {
      dwBytes = 0x10;
    }
    else {
      dwBytes = dwBytes + 0xf & 0xfffffff0;
    }
  }
  do {
    puVar3 = (undefined4 *)0x0;
    if (dwBytes < 0xffffffe1) {
      if (DAT_0043b66c < dwBytes) {
LAB_0041183d:
        if (puVar3 != (undefined4 *)0x0) {
          return puVar3;
        }
      }
      else {
        puVar3 = (undefined4 *)___sbh_alloc_block(dwBytes >> 4);
        if (puVar3 != (undefined4 *)0x0) {
          puVar4 = puVar3;
          for (uVar2 = dwBytes >> 2; uVar2 != 0; uVar2 = uVar2 - 1) {
            *puVar4 = 0;
            puVar4 = puVar4 + 1;
          }
          for (uVar2 = dwBytes & 3; uVar2 != 0; uVar2 = uVar2 - 1) {
            *(undefined1 *)puVar4 = 0;
            puVar4 = (undefined4 *)((int)puVar4 + 1);
          }
          goto LAB_0041183d;
        }
      }
      puVar3 = (undefined4 *)HeapAlloc(DAT_00454574,8,dwBytes);
    }
    if ((puVar3 != (undefined4 *)0x0) || (DAT_0043ae4c == 0)) {
      return puVar3;
    }
    iVar1 = __callnewh(dwBytes);
    if (iVar1 == 0) {
      return (void *)0x0;
    }
  } while( true );
}



// Library Function - Single Match
//  __setdefaultprecision
// 
// Library: Visual Studio 1998 Release

void __setdefaultprecision(void)

{
  __controlfp(0x10000,0x30000);
  return;
}



// Library Function - Single Match
//  __ms_p5_test_fdiv
// 
// Library: Visual Studio 1998 Release

undefined1 __ms_p5_test_fdiv(void)

{
  return 0;
}



// Library Function - Single Match
//  __ms_p5_mp_test_fdiv
// 
// Library: Visual Studio 1998 Release

void __ms_p5_mp_test_fdiv(void)

{
  HMODULE hModule;
  FARPROC pFVar1;
  
  hModule = GetModuleHandleA("KERNEL32");
  if (hModule != (HMODULE)0x0) {
    pFVar1 = GetProcAddress(hModule,"IsProcessorFeaturePresent");
    if (pFVar1 != (FARPROC)0x0) {
      (*pFVar1)(0);
      return;
    }
  }
  __ms_p5_test_fdiv();
  return;
}



// Library Function - Single Match
//  __cftoe
// 
// Library: Visual Studio 1998 Release

errno_t __cdecl __cftoe(double *_Value,char *_Buf,size_t _SizeInBytes,int _Dec,int _Caps)

{
  int *_Digits;
  char *pcVar1;
  char *pcVar2;
  int iVar3;
  STRFLT unaff_EBP;
  
  _Digits = DAT_0045275c;
  if (DAT_0043aad8 == '\0') {
    _Digits = (int *)__fltout();
    __fptostr(_Buf + (uint)(*_Digits == 0x2d) + (uint)(0 < (int)_SizeInBytes),_SizeInBytes + 1,
              (int)_Digits,unaff_EBP);
  }
  else {
    __shift(_Buf + (*DAT_0045275c == 0x2d),(uint)(0 < (int)_SizeInBytes));
  }
  pcVar1 = _Buf;
  if (*_Digits == 0x2d) {
    pcVar1 = _Buf + 1;
    *_Buf = '-';
  }
  pcVar2 = pcVar1;
  if (0 < (int)_SizeInBytes) {
    pcVar2 = pcVar1 + 1;
    *pcVar1 = *pcVar2;
    *pcVar2 = DAT_0043bb84;
  }
  builtin_strncpy(pcVar2 + _SizeInBytes + (DAT_0043aad8 == '\0'),"e+000",6);
  pcVar2 = pcVar2 + (DAT_0043aad8 == '\0') + _SizeInBytes;
  if (_Dec != 0) {
    *pcVar2 = 'E';
  }
  if (*(char *)_Digits[3] != '0') {
    iVar3 = _Digits[1] + -1;
    if (iVar3 < 0) {
      iVar3 = -iVar3;
      pcVar2[1] = '-';
    }
    if (99 < iVar3) {
      pcVar2[2] = pcVar2[2] + (char)(iVar3 / 100);
      iVar3 = iVar3 % 100;
    }
    if (9 < iVar3) {
      pcVar2[3] = pcVar2[3] + (char)(iVar3 / 10);
      iVar3 = iVar3 % 10;
    }
    pcVar2[4] = pcVar2[4] + (char)iVar3;
  }
  return (errno_t)_Buf;
}



// Library Function - Single Match
//  __cftof
// 
// Library: Visual Studio 1998 Release

errno_t __cdecl __cftof(double *_Value,char *_Buf,size_t _SizeInBytes,int _Dec)

{
  int iVar1;
  int *_Digits;
  uint uVar2;
  uint uVar3;
  STRFLT unaff_EBP;
  char *pcVar4;
  char *pcVar5;
  
  _Digits = DAT_0045275c;
  if (DAT_0043aad8 == '\0') {
    _Digits = (int *)__fltout();
    __fptostr(_Buf + (*_Digits == 0x2d),_Digits[1] + _SizeInBytes,(int)_Digits,unaff_EBP);
  }
  else if (DAT_0043aadc == _SizeInBytes) {
    iVar1 = DAT_0043aadc + (*DAT_0045275c == 0x2d);
    _Buf[iVar1] = '0';
    (_Buf + iVar1)[1] = '\0';
  }
  pcVar5 = _Buf;
  if (*_Digits == 0x2d) {
    pcVar5 = _Buf + 1;
    *_Buf = '-';
  }
  if (_Digits[1] < 1) {
    pcVar4 = pcVar5 + 1;
    __shift(pcVar5,1);
    *pcVar5 = '0';
  }
  else {
    pcVar4 = pcVar5 + _Digits[1];
  }
  if (0 < (int)_SizeInBytes) {
    __shift(pcVar4,1);
    *pcVar4 = DAT_0043bb84;
    iVar1 = _Digits[1];
    if (iVar1 < 0) {
      if (DAT_0043aad8 == '\0') {
        uVar3 = -iVar1;
        if ((int)_SizeInBytes <= -iVar1) {
          uVar3 = _SizeInBytes;
        }
      }
      else {
        uVar3 = -iVar1;
      }
      __shift(pcVar4 + 1,uVar3);
      uVar2 = uVar3 >> 2;
      pcVar5 = pcVar4 + 1;
      while (uVar2 != 0) {
        uVar2 = uVar2 - 1;
        builtin_strncpy(pcVar5,"0000",4);
        pcVar5 = pcVar5 + 4;
      }
      for (uVar3 = uVar3 & 3; uVar3 != 0; uVar3 = uVar3 - 1) {
        *pcVar5 = '0';
        pcVar5 = pcVar5 + 1;
      }
    }
  }
  return (errno_t)_Buf;
}



// Library Function - Single Match
//  __cftog
// 
// Library: Visual Studio 1998 Release

void __cdecl __cftog(double *param_1,char *param_2,size_t param_3,int param_4)

{
  int iVar1;
  char *pcVar2;
  int iVar3;
  char *pcVar4;
  STRFLT unaff_EBP;
  bool bVar5;
  
  DAT_0045275c = (int *)__fltout();
  DAT_0043aadc = DAT_0045275c[1] + -1;
  iVar1 = *DAT_0045275c;
  __fptostr(param_2 + (iVar1 == 0x2d),param_3,(int)DAT_0045275c,unaff_EBP);
  DAT_0043aae0 = DAT_0043aadc < DAT_0045275c[1] + -1;
  iVar3 = DAT_0045275c[1] + -1;
  if ((-5 < iVar3) && (iVar3 < (int)param_3)) {
    bVar5 = DAT_0043aadc < DAT_0045275c[1] + -1;
    pcVar2 = param_2 + (iVar1 == 0x2d);
    DAT_0043aadc = iVar3;
    if (bVar5) {
      do {
        pcVar4 = pcVar2;
        pcVar2 = pcVar4 + 1;
      } while (*pcVar4 != '\0');
      pcVar4[-1] = '\0';
    }
    __cftof_g(param_1,param_2,param_3);
    return;
  }
  DAT_0043aadc = iVar3;
  __cftoe_g(param_1,param_2,param_3,param_4);
  return;
}



// Library Function - Single Match
//  __cftoe_g
// 
// Library: Visual Studio 1998 Release

void __cdecl __cftoe_g(double *param_1,char *param_2,size_t param_3,int param_4)

{
  int unaff_retaddr;
  
  DAT_0043aad8 = 1;
  __cftoe(param_1,param_2,param_3,param_4,unaff_retaddr);
  DAT_0043aad8 = 0;
  return;
}



// Library Function - Single Match
//  __cftof_g
// 
// Library: Visual Studio 1998 Release

void __cdecl __cftof_g(double *param_1,char *param_2,size_t param_3)

{
  int unaff_retaddr;
  
  DAT_0043aad8 = 1;
  __cftof(param_1,param_2,param_3,unaff_retaddr);
  DAT_0043aad8 = 0;
  return;
}



// Library Function - Single Match
//  __shift
// 
// Library: Visual Studio 1998 Release

void __cdecl __shift(char *param_1,int param_2)

{
  char cVar1;
  uint uVar2;
  char *pcVar3;
  
  if (param_2 != 0) {
    uVar2 = 0xffffffff;
    pcVar3 = param_1;
    do {
      if (uVar2 == 0) break;
      uVar2 = uVar2 - 1;
      cVar1 = *pcVar3;
      pcVar3 = pcVar3 + 1;
    } while (cVar1 != '\0');
    FID_conflict__memcpy(param_1 + param_2,param_1,~uVar2);
  }
  return;
}



// Library Function - Single Match
//  __global_unwind2
// 
// Library: Visual Studio

void __cdecl __global_unwind2(PVOID param_1)

{
  RtlUnwind(param_1,(PVOID)0x411fe4,(PEXCEPTION_RECORD)0x0,(PVOID)0x0);
  return;
}



// Library Function - Single Match
//  __local_unwind2
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release, Visual Studio 2003 Debug, Visual
// Studio 2003 Release

void __cdecl __local_unwind2(int param_1,int param_2)

{
  int iVar1;
  int iVar2;
  undefined4 *unaff_FS_OFFSET;
  undefined4 uStack_1c;
  undefined1 *puStack_18;
  undefined4 local_14;
  int iStack_10;
  
  iStack_10 = param_1;
  puStack_18 = &LAB_00411fec;
  uStack_1c = *unaff_FS_OFFSET;
  *unaff_FS_OFFSET = &uStack_1c;
  while( true ) {
    iVar1 = *(int *)(param_1 + 8);
    iVar2 = *(int *)(param_1 + 0xc);
    if ((iVar2 == -1) || (iVar2 == param_2)) break;
    local_14 = *(undefined4 *)(iVar1 + iVar2 * 0xc);
    *(undefined4 *)(param_1 + 0xc) = local_14;
    if (*(int *)(iVar1 + 4 + iVar2 * 0xc) == 0) {
      FUN_004120a2();
      (**(code **)(iVar1 + 8 + iVar2 * 0xc))();
    }
  }
  *unaff_FS_OFFSET = uStack_1c;
  return;
}



void FUN_004120a2(void)

{
  undefined4 in_EAX;
  int unaff_EBP;
  
  DAT_0043aaec = *(undefined4 *)(unaff_EBP + 8);
  DAT_0043aae8 = in_EAX;
  DAT_0043aaf0 = unaff_EBP;
  return;
}



// Library Function - Single Match
//  __ismbblead
// 
// Library: Visual Studio 1998 Release

int __cdecl __ismbblead(uint _C)

{
  int iVar1;
  
  iVar1 = x_ismbbtype((byte)_C,0,4);
  return iVar1;
}



// Library Function - Single Match
//  _x_ismbbtype
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl x_ismbbtype(byte param_1,uint param_2,byte param_3)

{
  uint uVar1;
  
  if ((param_3 & (&DAT_0043ab91)[param_1]) == 0) {
    uVar1 = 0;
    if (param_2 != 0) {
      uVar1 = *(ushort *)(&DAT_0043bb9a + (uint)param_1 * 2) & param_2;
    }
    if (uVar1 == 0) {
      return 0;
    }
  }
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __setenvp
// 
// Library: Visual Studio 1998 Release

int __cdecl __setenvp(void)

{
  char cVar1;
  undefined4 *puVar2;
  void *pvVar3;
  char *pcVar4;
  uint uVar5;
  uint uVar6;
  uint uVar7;
  char *pcVar8;
  int iVar9;
  char *pcVar10;
  
  iVar9 = 0;
  cVar1 = *DAT_0043aaa4;
  pcVar8 = DAT_0043aaa4;
  while (cVar1 != '\0') {
    if (*pcVar8 != '=') {
      iVar9 = iVar9 + 1;
    }
    uVar5 = 0xffffffff;
    pcVar4 = pcVar8;
    do {
      if (uVar5 == 0) break;
      uVar5 = uVar5 - 1;
      cVar1 = *pcVar4;
      pcVar4 = pcVar4 + 1;
    } while (cVar1 != '\0');
    pcVar8 = pcVar8 + ~uVar5;
    cVar1 = *pcVar8;
  }
  puVar2 = (undefined4 *)_malloc(iVar9 * 4 + 4);
  _DAT_0043aa80 = puVar2;
  if (puVar2 == (undefined4 *)0x0) {
    __amsg_exit(9);
  }
  cVar1 = *DAT_0043aaa4;
  pcVar8 = DAT_0043aaa4;
  pcVar4 = DAT_0043aaa4;
  do {
    DAT_0043aaa4 = pcVar4;
    if (cVar1 == '\0') {
      _free(pcVar4);
      DAT_0043aaa4 = (char *)0x0;
      *puVar2 = 0;
      return (int)pcVar4;
    }
    uVar5 = 0xffffffff;
    pcVar4 = pcVar8;
    do {
      if (uVar5 == 0) break;
      uVar5 = uVar5 - 1;
      cVar1 = *pcVar4;
      pcVar4 = pcVar4 + 1;
    } while (cVar1 != '\0');
    if (*pcVar8 != '=') {
      pvVar3 = _malloc(~uVar5);
      *puVar2 = pvVar3;
      if (pvVar3 == (void *)0x0) {
        __amsg_exit(9);
      }
      uVar6 = 0xffffffff;
      pcVar4 = pcVar8;
      do {
        pcVar10 = pcVar4;
        if (uVar6 == 0) break;
        uVar6 = uVar6 - 1;
        pcVar10 = pcVar4 + 1;
        cVar1 = *pcVar4;
        pcVar4 = pcVar10;
      } while (cVar1 != '\0');
      uVar6 = ~uVar6;
      pcVar4 = (char *)*puVar2;
      puVar2 = puVar2 + 1;
      pcVar10 = pcVar10 + -uVar6;
      for (uVar7 = uVar6 >> 2; uVar7 != 0; uVar7 = uVar7 - 1) {
        *(undefined4 *)pcVar4 = *(undefined4 *)pcVar10;
        pcVar10 = pcVar10 + 4;
        pcVar4 = pcVar4 + 4;
      }
      for (uVar6 = uVar6 & 3; uVar6 != 0; uVar6 = uVar6 - 1) {
        *pcVar4 = *pcVar10;
        pcVar10 = pcVar10 + 1;
        pcVar4 = pcVar4 + 1;
      }
    }
    pcVar8 = pcVar8 + ~uVar5;
    cVar1 = *pcVar8;
    pcVar4 = DAT_0043aaa4;
  } while( true );
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __setargv
// 
// Library: Visual Studio 1998 Release

int __cdecl __setargv(void)

{
  undefined4 *puVar1;
  byte *pbVar2;
  int iStack_8;
  int iStack_4;
  
  GetModuleFileNameA((HMODULE)0x0,&DAT_00452760,0x104);
  _DAT_0043aa90 = &DAT_00452760;
  pbVar2 = &DAT_00452760;
  if (*DAT_00454684 != 0) {
    pbVar2 = DAT_00454684;
  }
  parse_cmdline(pbVar2,(undefined4 *)0x0,(byte *)0x0,&iStack_8,&iStack_4);
  puVar1 = (undefined4 *)_malloc(iStack_8 * 4 + iStack_4);
  if (puVar1 == (undefined4 *)0x0) {
    __amsg_exit(8);
  }
  parse_cmdline(pbVar2,puVar1,(byte *)(puVar1 + iStack_8),&iStack_8,&iStack_4);
  _DAT_0043aa78 = puVar1;
  _DAT_0043aa74 = iStack_8 + -1;
  return iStack_8 + -1;
}



// Library Function - Single Match
//  _parse_cmdline
// 
// Library: Visual Studio 1998 Release

void __cdecl
parse_cmdline(byte *param_1,undefined4 *param_2,byte *param_3,int *param_4,int *param_5)

{
  byte bVar1;
  bool bVar2;
  bool bVar3;
  byte *pbVar4;
  uint uVar5;
  byte *pbVar6;
  
  *param_5 = 0;
  *param_4 = 1;
  if (param_2 != (undefined4 *)0x0) {
    *param_2 = param_3;
    param_2 = param_2 + 1;
  }
  if (*param_1 == 0x22) {
    pbVar6 = param_1 + 1;
    bVar1 = *pbVar6;
    while ((bVar1 != 0x22 && (*pbVar6 != 0))) {
      if ((((&DAT_0043ab91)[*pbVar6] & 4) != 0) && (*param_5 = *param_5 + 1, param_3 != (byte *)0x0)
         ) {
        bVar1 = *pbVar6;
        pbVar6 = pbVar6 + 1;
        *param_3 = bVar1;
        param_3 = param_3 + 1;
      }
      *param_5 = *param_5 + 1;
      if (param_3 != (byte *)0x0) {
        *param_3 = *pbVar6;
        param_3 = param_3 + 1;
      }
      pbVar6 = pbVar6 + 1;
      bVar1 = *pbVar6;
    }
    *param_5 = *param_5 + 1;
    if (param_3 != (byte *)0x0) {
      *param_3 = 0;
      param_3 = param_3 + 1;
    }
    if (*pbVar6 == 0x22) {
      pbVar6 = pbVar6 + 1;
    }
  }
  else {
    do {
      *param_5 = *param_5 + 1;
      if (param_3 != (byte *)0x0) {
        *param_3 = *param_1;
        param_3 = param_3 + 1;
      }
      bVar1 = *param_1;
      pbVar6 = param_1 + 1;
      if (((&DAT_0043ab91)[bVar1] & 4) != 0) {
        *param_5 = *param_5 + 1;
        if (param_3 != (byte *)0x0) {
          *param_3 = *pbVar6;
          param_3 = param_3 + 1;
        }
        pbVar6 = param_1 + 2;
      }
      if (bVar1 == 0x20) break;
      if (bVar1 == 0) goto LAB_004124b0;
      param_1 = pbVar6;
    } while (bVar1 != 9);
    if (bVar1 == 0) {
LAB_004124b0:
      pbVar6 = pbVar6 + -1;
    }
    else if (param_3 != (byte *)0x0) {
      param_3[-1] = 0;
    }
  }
  bVar3 = false;
  while (*pbVar6 != 0) {
    for (; (*pbVar6 == 0x20 || (*pbVar6 == 9)); pbVar6 = pbVar6 + 1) {
    }
    if (*pbVar6 == 0) break;
    if (param_2 != (undefined4 *)0x0) {
      *param_2 = param_3;
      param_2 = param_2 + 1;
    }
    *param_4 = *param_4 + 1;
    while( true ) {
      bVar2 = true;
      uVar5 = 0;
      bVar1 = *pbVar6;
      while (bVar1 == 0x5c) {
        pbVar6 = pbVar6 + 1;
        uVar5 = uVar5 + 1;
        bVar1 = *pbVar6;
      }
      if (*pbVar6 == 0x22) {
        pbVar4 = pbVar6;
        if ((uVar5 & 1) == 0) {
          if ((!bVar3) || (pbVar4 = pbVar6 + 1, *pbVar4 != 0x22)) {
            bVar2 = false;
            pbVar4 = pbVar6;
          }
          bVar3 = !bVar3;
        }
        uVar5 = uVar5 >> 1;
        pbVar6 = pbVar4;
      }
      while (uVar5 != 0) {
        uVar5 = uVar5 - 1;
        if (param_3 != (byte *)0x0) {
          *param_3 = 0x5c;
          param_3 = param_3 + 1;
        }
        *param_5 = *param_5 + 1;
      }
      bVar1 = *pbVar6;
      if ((bVar1 == 0) || ((!bVar3 && ((bVar1 == 0x20 || (bVar1 == 9)))))) break;
      if (bVar2) {
        if (param_3 == (byte *)0x0) {
          if (((&DAT_0043ab91)[bVar1] & 4) != 0) {
            pbVar6 = pbVar6 + 1;
            *param_5 = *param_5 + 1;
          }
          *param_5 = *param_5 + 1;
          goto LAB_004125e1;
        }
        pbVar4 = param_3;
        if (((&DAT_0043ab91)[bVar1] & 4) != 0) {
          *param_3 = bVar1;
          pbVar6 = pbVar6 + 1;
          pbVar4 = param_3 + 1;
          *param_5 = *param_5 + 1;
        }
        bVar1 = *pbVar6;
        param_3 = pbVar4 + 1;
        pbVar6 = pbVar6 + 1;
        *pbVar4 = bVar1;
        *param_5 = *param_5 + 1;
      }
      else {
LAB_004125e1:
        pbVar6 = pbVar6 + 1;
      }
    }
    if (param_3 != (byte *)0x0) {
      *param_3 = 0;
      param_3 = param_3 + 1;
    }
    *param_5 = *param_5 + 1;
  }
  if (param_2 != (undefined4 *)0x0) {
    *param_2 = 0;
  }
  *param_4 = *param_4 + 1;
  return;
}



// Library Function - Single Match
//  ___crtGetEnvironmentStringsA
// 
// Library: Visual Studio 1998 Release

LPVOID __cdecl ___crtGetEnvironmentStringsA(void)

{
  char cVar1;
  WCHAR WVar2;
  size_t _Size;
  LPSTR lpMultiByteStr;
  CHAR *pCVar3;
  uint uVar4;
  LPCH pCVar5;
  char *pcVar6;
  WCHAR *pWVar8;
  int iVar10;
  LPCH pCVar11;
  LPWCH lpWideCharStr;
  CHAR *pCVar12;
  char *pcVar7;
  WCHAR *pWVar9;
  
  pCVar5 = (LPCH)0x0;
  lpWideCharStr = (LPWCH)0x0;
  if (DAT_0043ab88 == 0) {
    lpWideCharStr = GetEnvironmentStringsW();
    if (lpWideCharStr == (LPWCH)0x0) {
      pCVar5 = GetEnvironmentStrings();
      if (pCVar5 == (LPCH)0x0) {
        return (LPVOID)0x0;
      }
      DAT_0043ab88 = 2;
    }
    else {
      DAT_0043ab88 = 1;
    }
  }
  if (DAT_0043ab88 != 1) {
    if (DAT_0043ab88 != 2) {
      return (LPVOID)0x0;
    }
    if ((pCVar5 == (LPCH)0x0) && (pCVar5 = GetEnvironmentStrings(), pCVar5 == (LPCH)0x0)) {
      return (LPVOID)0x0;
    }
    cVar1 = *pCVar5;
    pcVar6 = pCVar5;
    while (cVar1 != '\0') {
      do {
        pcVar7 = pcVar6;
        pcVar6 = pcVar7 + 1;
      } while (*pcVar6 != '\0');
      pcVar6 = pcVar7 + 2;
      cVar1 = *pcVar6;
    }
    pcVar6 = pcVar6 + (1 - (int)pCVar5);
    pCVar3 = (CHAR *)_malloc((size_t)pcVar6);
    if (pCVar3 != (CHAR *)0x0) {
      pCVar11 = pCVar5;
      pCVar12 = pCVar3;
      for (uVar4 = (uint)pcVar6 >> 2; uVar4 != 0; uVar4 = uVar4 - 1) {
        *(undefined4 *)pCVar12 = *(undefined4 *)pCVar11;
        pCVar11 = pCVar11 + 4;
        pCVar12 = pCVar12 + 4;
      }
      for (uVar4 = (uint)pcVar6 & 3; uVar4 != 0; uVar4 = uVar4 - 1) {
        *pCVar12 = *pCVar11;
        pCVar11 = pCVar11 + 1;
        pCVar12 = pCVar12 + 1;
      }
      FreeEnvironmentStringsA(pCVar5);
      return pCVar3;
    }
    FreeEnvironmentStringsA(pCVar5);
    return (LPVOID)0x0;
  }
  if ((lpWideCharStr == (LPWCH)0x0) &&
     (lpWideCharStr = GetEnvironmentStringsW(), lpWideCharStr == (LPWCH)0x0)) {
    return (LPVOID)0x0;
  }
  WVar2 = *lpWideCharStr;
  pWVar8 = lpWideCharStr;
  while (WVar2 != L'\0') {
    do {
      pWVar9 = pWVar8;
      pWVar8 = pWVar9 + 1;
    } while (*pWVar8 != L'\0');
    pWVar8 = pWVar9 + 2;
    WVar2 = *pWVar8;
  }
  iVar10 = ((int)pWVar8 - (int)lpWideCharStr >> 1) + 1;
  _Size = WideCharToMultiByte(0,0,lpWideCharStr,iVar10,(LPSTR)0x0,0,(LPCSTR)0x0,(LPBOOL)0x0);
  if ((_Size != 0) && (lpMultiByteStr = (LPSTR)_malloc(_Size), lpMultiByteStr != (LPSTR)0x0)) {
    iVar10 = WideCharToMultiByte(0,0,lpWideCharStr,iVar10,lpMultiByteStr,_Size,(LPCSTR)0x0,
                                 (LPBOOL)0x0);
    if (iVar10 == 0) {
      _free(lpMultiByteStr);
      lpMultiByteStr = (LPSTR)0x0;
    }
    FreeEnvironmentStringsW(lpWideCharStr);
    return lpMultiByteStr;
  }
  FreeEnvironmentStringsW(lpWideCharStr);
  return (LPVOID)0x0;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __setmbcp
// 
// Library: Visual Studio 1998 Release

int __cdecl __setmbcp(int _CodePage)

{
  byte bVar1;
  UINT CodePage;
  UINT *pUVar2;
  BOOL BVar3;
  int iVar4;
  uint uVar5;
  uint uVar6;
  BYTE *pBVar7;
  byte *pbVar8;
  undefined4 *puVar9;
  int local_18;
  _cpinfo local_14;
  
  CodePage = getSystemCP(_CodePage);
  if (CodePage == DAT_0043ac94) {
    return 0;
  }
  if (CodePage == 0) {
    setSBCS();
    return 0;
  }
  local_18 = 0;
  pUVar2 = &DAT_0043acb8;
  do {
    if (*pUVar2 == CodePage) {
      uVar5 = 0;
      puVar9 = (undefined4 *)&DAT_0043ab90;
      for (iVar4 = 0x40; iVar4 != 0; iVar4 = iVar4 + -1) {
        *puVar9 = 0;
        puVar9 = puVar9 + 1;
      }
      *(undefined1 *)puVar9 = 0;
      do {
        pbVar8 = &DAT_0043acc8 + (uVar5 + local_18 * 6) * 8;
        bVar1 = *pbVar8;
        while ((bVar1 != 0 && (pbVar8[1] != 0))) {
          uVar6 = (uint)*pbVar8;
          if (uVar6 <= pbVar8[1]) {
            bVar1 = (&DAT_0043acb0)[uVar5];
            do {
              (&DAT_0043ab91)[uVar6] = (&DAT_0043ab91)[uVar6] | bVar1;
              uVar6 = uVar6 + 1;
            } while (uVar6 <= pbVar8[1]);
          }
          pbVar8 = pbVar8 + 2;
          bVar1 = *pbVar8;
        }
        uVar5 = uVar5 + 1;
      } while (uVar5 < 4);
      DAT_0043ac94 = CodePage;
      _DAT_0043ac98 = _CPtoLCID(CodePage);
      DAT_0043aca0 = *(undefined4 *)(&DAT_0043acbc + local_18 * 0x30);
      DAT_0043aca4 = *(undefined4 *)(&DAT_0043acc0 + local_18 * 0x30);
      DAT_0043aca8 = *(undefined4 *)(local_18 * 0x30 + 0x43acc4);
      return 0;
    }
    pUVar2 = pUVar2 + 0xc;
    local_18 = local_18 + 1;
  } while (pUVar2 < &DAT_0043ada8);
  BVar3 = GetCPInfo(CodePage,&local_14);
  if (BVar3 == 1) {
    puVar9 = (undefined4 *)&DAT_0043ab90;
    for (iVar4 = 0x40; iVar4 != 0; iVar4 = iVar4 + -1) {
      *puVar9 = 0;
      puVar9 = puVar9 + 1;
    }
    *(undefined1 *)puVar9 = 0;
    if (local_14.MaxCharSize < 2) {
      _DAT_0043ac98 = 0;
      DAT_0043ac94 = 0;
    }
    else {
      pBVar7 = local_14.LeadByte;
      while ((local_14.LeadByte[0] != 0 && (pBVar7[1] != 0))) {
        uVar5 = (uint)*pBVar7;
        if (uVar5 <= pBVar7[1]) {
          do {
            (&DAT_0043ab91)[uVar5] = (&DAT_0043ab91)[uVar5] | 4;
            uVar5 = uVar5 + 1;
          } while (uVar5 <= pBVar7[1]);
        }
        pBVar7 = pBVar7 + 2;
        local_14.LeadByte[0] = *pBVar7;
      }
      uVar5 = 1;
      do {
        (&DAT_0043ab91)[uVar5] = (&DAT_0043ab91)[uVar5] | 8;
        uVar5 = uVar5 + 1;
      } while (uVar5 < 0xff);
      DAT_0043ac94 = CodePage;
      _DAT_0043ac98 = _CPtoLCID(CodePage);
    }
    DAT_0043aca0 = 0;
    DAT_0043aca4 = 0;
    DAT_0043aca8 = 0;
    return 0;
  }
  if (DAT_0043acac == 0) {
    return -1;
  }
  setSBCS();
  return 0;
}



// Library Function - Single Match
//  _getSystemCP
// 
// Library: Visual Studio 1998 Release

int __cdecl getSystemCP(int param_1)

{
  int iVar1;
  bool bVar2;
  
  if (param_1 == -2) {
    DAT_0043acac = 1;
                    // WARNING: Could not recover jumptable at 0x004129ad. Too many branches
                    // WARNING: Treating indirect jump as call
    iVar1 = GetOEMCP();
    return iVar1;
  }
  if (param_1 == -3) {
    DAT_0043acac = 1;
                    // WARNING: Could not recover jumptable at 0x004129c2. Too many branches
                    // WARNING: Treating indirect jump as call
    iVar1 = GetACP();
    return iVar1;
  }
  bVar2 = param_1 == -4;
  if (bVar2) {
    param_1 = DAT_0043bde8;
  }
  DAT_0043acac = (uint)bVar2;
  return param_1;
}



// Library Function - Single Match
//  _CPtoLCID
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl _CPtoLCID(undefined4 param_1)

{
  switch(param_1) {
  case 0x3a4:
    return 0x411;
  default:
    return 0;
  case 0x3a8:
    return 0x804;
  case 0x3b5:
    return 0x412;
  case 0x3b6:
    return 0x404;
  }
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  _setSBCS
// 
// Library: Visual Studio 1998 Release

void __cdecl setSBCS(void)

{
  int iVar1;
  undefined4 *puVar2;
  
  puVar2 = (undefined4 *)&DAT_0043ab90;
  for (iVar1 = 0x40; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar2 = 0;
    puVar2 = puVar2 + 1;
  }
  *(undefined1 *)puVar2 = 0;
  DAT_0043aca0 = 0;
  DAT_0043ac94 = 0;
  _DAT_0043ac98 = 0;
  DAT_0043aca4 = 0;
  DAT_0043aca8 = 0;
  return;
}



// Library Function - Single Match
//  ___initmbctable
// 
// Library: Visual Studio 1998 Release

void ___initmbctable(void)

{
  __setmbcp(-3);
  return;
}



// Library Function - Single Match
//  __ioinit
// 
// Library: Visual Studio 1998 Release

int __cdecl __ioinit(void)

{
  undefined4 *puVar1;
  DWORD DVar2;
  HANDLE hFile;
  byte *pbVar3;
  int *piVar4;
  uint uVar5;
  undefined4 *puVar6;
  UINT UVar7;
  UINT UVar8;
  int iVar9;
  UINT *pUVar10;
  _STARTUPINFOA local_44;
  
  puVar1 = (undefined4 *)_malloc(0x100);
  if (puVar1 == (undefined4 *)0x0) {
    __amsg_exit(0x1b);
  }
  DAT_00454680 = 0x20;
  DAT_00454580 = puVar1;
  if (puVar1 < puVar1 + 0x40) {
    do {
      *(undefined1 *)(puVar1 + 1) = 0;
      puVar6 = puVar1 + 2;
      *puVar1 = 0xffffffff;
      *(undefined1 *)((int)puVar1 + 5) = 10;
      puVar1 = puVar6;
    } while (puVar6 < DAT_00454580 + 0x40);
  }
  GetStartupInfoA(&local_44);
  if ((local_44.cbReserved2 != 0) && ((UINT *)local_44.lpReserved2 != (UINT *)0x0)) {
    UVar7 = *(UINT *)local_44.lpReserved2;
    pUVar10 = (UINT *)((int)local_44.lpReserved2 + 4);
    pbVar3 = (byte *)(UVar7 + (int)pUVar10);
    if (0x7ff < (int)UVar7) {
      UVar7 = 0x800;
    }
    UVar8 = UVar7;
    if ((int)DAT_00454680 < (int)UVar7) {
      piVar4 = &DAT_00454584;
      do {
        puVar1 = (undefined4 *)_malloc(0x100);
        UVar8 = DAT_00454680;
        if (puVar1 == (undefined4 *)0x0) break;
        *piVar4 = (int)puVar1;
        DAT_00454680 = DAT_00454680 + 0x20;
        if (puVar1 < puVar1 + 0x40) {
          do {
            *(undefined1 *)(puVar1 + 1) = 0;
            puVar6 = puVar1 + 2;
            *puVar1 = 0xffffffff;
            *(undefined1 *)((int)puVar1 + 5) = 10;
            puVar1 = puVar6;
          } while (puVar6 < (undefined4 *)(*piVar4 + 0x100));
        }
        piVar4 = piVar4 + 1;
        UVar8 = UVar7;
      } while ((int)DAT_00454680 < (int)UVar7);
    }
    uVar5 = 0;
    if (0 < (int)UVar8) {
      do {
        if (((*(HANDLE *)pbVar3 != (HANDLE)0xffffffff) && ((*pUVar10 & 1) != 0)) &&
           (((*pUVar10 & 8) != 0 || (DVar2 = GetFileType(*(HANDLE *)pbVar3), DVar2 != 0)))) {
          puVar1 = (undefined4 *)
                   (*(int *)((int)&DAT_00454580 + ((int)(uVar5 & 0xffffffe7) >> 3)) +
                   (uVar5 & 0x1f) * 8);
          *puVar1 = *(undefined4 *)pbVar3;
          *(byte *)(puVar1 + 1) = (byte)*pUVar10;
        }
        uVar5 = uVar5 + 1;
        pUVar10 = (UINT *)((int)pUVar10 + 1);
        pbVar3 = pbVar3 + 4;
      } while ((int)uVar5 < (int)UVar8);
    }
  }
  iVar9 = 0;
  do {
    piVar4 = DAT_00454580 + iVar9 * 2;
    if (*piVar4 == -1) {
      DVar2 = 0xfffffff6;
      *(undefined1 *)(piVar4 + 1) = 0x81;
      if (iVar9 != 0) {
        DVar2 = (iVar9 == 1) - 0xc;
      }
      hFile = GetStdHandle(DVar2);
      if ((hFile == (HANDLE)0xffffffff) || (DVar2 = GetFileType(hFile), DVar2 == 0)) {
        *(byte *)(piVar4 + 1) = *(byte *)(piVar4 + 1) | 0x40;
      }
      else {
        *piVar4 = (int)hFile;
        if ((DVar2 & 0xff) == 2) {
          *(byte *)(piVar4 + 1) = *(byte *)(piVar4 + 1) | 0x40;
        }
        else if ((DVar2 & 0xff) == 3) {
          *(byte *)(piVar4 + 1) = *(byte *)(piVar4 + 1) | 8;
        }
      }
    }
    else {
      *(byte *)(piVar4 + 1) = *(byte *)(piVar4 + 1) | 0x80;
    }
    iVar9 = iVar9 + 1;
  } while (iVar9 < 3);
  UVar7 = SetHandleCount(DAT_00454680);
  return UVar7;
}



// Library Function - Single Match
//  __heap_init
// 
// Library: Visual Studio 1998 Release

int __cdecl __heap_init(void)

{
  undefined **ppuVar1;
  
  DAT_00454574 = HeapCreate(1,0x1000,0);
  if (DAT_00454574 == (HANDLE)0x0) {
    return 0;
  }
  ppuVar1 = ___sbh_new_region();
  if (ppuVar1 == (undefined **)0x0) {
    HeapDestroy(DAT_00454574);
    return 0;
  }
  return 1;
}



void FUN_00412d61(int param_1)

{
  __local_unwind2(*(int *)(param_1 + 0x18),*(int *)(param_1 + 0x1c));
  return;
}



// Library Function - Single Match
//  __FF_MSGBANNER
// 
// Library: Visual Studio 1998 Release

void __cdecl __FF_MSGBANNER(void)

{
  if ((DAT_0043aab0 == 1) || ((DAT_0043aab0 == 0 && (DAT_0043aab4 == 1)))) {
    __NMSG_WRITE(0xfc);
    if (DAT_0043ae48 != (code *)0x0) {
      (*DAT_0043ae48)();
    }
    __NMSG_WRITE(0xff);
  }
  return;
}



// Library Function - Single Match
//  __NMSG_WRITE
// 
// Library: Visual Studio 1998 Release

void __cdecl __NMSG_WRITE(int param_1)

{
  char cVar1;
  int *piVar2;
  DWORD DVar3;
  HANDLE hFile;
  int iVar4;
  int iVar5;
  uint uVar6;
  uint uVar7;
  char *pcVar8;
  char *pcVar9;
  CHAR *pCVar10;
  char *pcVar11;
  DWORD local_1a8;
  char local_1a4 [100];
  char acStack_140 [60];
  CHAR local_104 [260];
  
  iVar4 = 0;
  piVar2 = &DAT_0043adb8;
  do {
    if (*piVar2 == param_1) break;
    piVar2 = piVar2 + 2;
    iVar4 = iVar4 + 1;
  } while (piVar2 < &DAT_0043ae48);
  if ((&DAT_0043adb8)[iVar4 * 2] == param_1) {
    if ((DAT_0043aab0 == 1) || ((DAT_0043aab0 == 0 && (DAT_0043aab4 == 1)))) {
      if ((DAT_00454580 == 0) ||
         (hFile = *(HANDLE *)(DAT_00454580 + 0x10), hFile == (HANDLE)0xffffffff)) {
        hFile = GetStdHandle(0xfffffff4);
      }
      pcVar8 = *(char **)(iVar4 * 8 + 0x43adbc);
      uVar6 = 0xffffffff;
      pcVar9 = pcVar8;
      do {
        if (uVar6 == 0) break;
        uVar6 = uVar6 - 1;
        cVar1 = *pcVar9;
        pcVar9 = pcVar9 + 1;
      } while (cVar1 != '\0');
      WriteFile(hFile,pcVar8,~uVar6 - 1,&local_1a8,(LPOVERLAPPED)0x0);
    }
    else if (param_1 != 0xfc) {
      DVar3 = GetModuleFileNameA((HMODULE)0x0,local_104,0x104);
      if (DVar3 == 0) {
        pcVar8 = "<program name unknown>";
        pCVar10 = local_104;
        for (iVar5 = 5; iVar5 != 0; iVar5 = iVar5 + -1) {
          *(undefined4 *)pCVar10 = *(undefined4 *)pcVar8;
          pcVar8 = pcVar8 + 4;
          pCVar10 = pCVar10 + 4;
        }
        *(undefined2 *)pCVar10 = *(undefined2 *)pcVar8;
        pCVar10[2] = pcVar8[2];
      }
      pcVar8 = local_104;
      uVar6 = 0xffffffff;
      pcVar9 = local_104;
      do {
        if (uVar6 == 0) break;
        uVar6 = uVar6 - 1;
        cVar1 = *pcVar9;
        pcVar9 = pcVar9 + 1;
      } while (cVar1 != '\0');
      if (0x3c < ~uVar6) {
        uVar6 = 0xffffffff;
        pcVar8 = local_104;
        do {
          if (uVar6 == 0) break;
          uVar6 = uVar6 - 1;
          cVar1 = *pcVar8;
          pcVar8 = pcVar8 + 1;
        } while (cVar1 != '\0');
        pcVar8 = acStack_140 + ~uVar6;
        _strncpy(pcVar8,"...",3);
      }
      pcVar9 = "Runtime Error!\n\nProgram: ";
      pcVar11 = local_1a4;
      for (iVar5 = 6; iVar5 != 0; iVar5 = iVar5 + -1) {
        *(undefined4 *)pcVar11 = *(undefined4 *)pcVar9;
        pcVar9 = pcVar9 + 4;
        pcVar11 = pcVar11 + 4;
      }
      *(undefined2 *)pcVar11 = *(undefined2 *)pcVar9;
      uVar6 = 0xffffffff;
      do {
        pcVar9 = pcVar8;
        if (uVar6 == 0) break;
        uVar6 = uVar6 - 1;
        pcVar9 = pcVar8 + 1;
        cVar1 = *pcVar8;
        pcVar8 = pcVar9;
      } while (cVar1 != '\0');
      uVar6 = ~uVar6;
      iVar5 = -1;
      pcVar8 = local_1a4;
      do {
        pcVar11 = pcVar8;
        if (iVar5 == 0) break;
        iVar5 = iVar5 + -1;
        pcVar11 = pcVar8 + 1;
        cVar1 = *pcVar8;
        pcVar8 = pcVar11;
      } while (cVar1 != '\0');
      pcVar8 = pcVar9 + -uVar6;
      pcVar9 = pcVar11 + -1;
      for (uVar7 = uVar6 >> 2; uVar7 != 0; uVar7 = uVar7 - 1) {
        *(undefined4 *)pcVar9 = *(undefined4 *)pcVar8;
        pcVar8 = pcVar8 + 4;
        pcVar9 = pcVar9 + 4;
      }
      for (uVar6 = uVar6 & 3; uVar6 != 0; uVar6 = uVar6 - 1) {
        *pcVar9 = *pcVar8;
        pcVar8 = pcVar8 + 1;
        pcVar9 = pcVar9 + 1;
      }
      uVar6 = 0xffffffff;
      pcVar8 = "\n\n";
      do {
        pcVar9 = pcVar8;
        if (uVar6 == 0) break;
        uVar6 = uVar6 - 1;
        pcVar9 = pcVar8 + 1;
        cVar1 = *pcVar8;
        pcVar8 = pcVar9;
      } while (cVar1 != '\0');
      uVar6 = ~uVar6;
      iVar5 = -1;
      pcVar8 = local_1a4;
      do {
        pcVar11 = pcVar8;
        if (iVar5 == 0) break;
        iVar5 = iVar5 + -1;
        pcVar11 = pcVar8 + 1;
        cVar1 = *pcVar8;
        pcVar8 = pcVar11;
      } while (cVar1 != '\0');
      pcVar8 = pcVar9 + -uVar6;
      pcVar9 = pcVar11 + -1;
      for (uVar7 = uVar6 >> 2; uVar7 != 0; uVar7 = uVar7 - 1) {
        *(undefined4 *)pcVar9 = *(undefined4 *)pcVar8;
        pcVar8 = pcVar8 + 4;
        pcVar9 = pcVar9 + 4;
      }
      for (uVar6 = uVar6 & 3; uVar6 != 0; uVar6 = uVar6 - 1) {
        *pcVar9 = *pcVar8;
        pcVar8 = pcVar8 + 1;
        pcVar9 = pcVar9 + 1;
      }
      uVar6 = 0xffffffff;
      pcVar8 = *(char **)(iVar4 * 8 + 0x43adbc);
      do {
        pcVar9 = pcVar8;
        if (uVar6 == 0) break;
        uVar6 = uVar6 - 1;
        pcVar9 = pcVar8 + 1;
        cVar1 = *pcVar8;
        pcVar8 = pcVar9;
      } while (cVar1 != '\0');
      uVar6 = ~uVar6;
      iVar4 = -1;
      pcVar8 = local_1a4;
      do {
        pcVar11 = pcVar8;
        if (iVar4 == 0) break;
        iVar4 = iVar4 + -1;
        pcVar11 = pcVar8 + 1;
        cVar1 = *pcVar8;
        pcVar8 = pcVar11;
      } while (cVar1 != '\0');
      pcVar8 = pcVar9 + -uVar6;
      pcVar9 = pcVar11 + -1;
      for (uVar7 = uVar6 >> 2; uVar7 != 0; uVar7 = uVar7 - 1) {
        *(undefined4 *)pcVar9 = *(undefined4 *)pcVar8;
        pcVar8 = pcVar8 + 4;
        pcVar9 = pcVar9 + 4;
      }
      for (uVar6 = uVar6 & 3; uVar6 != 0; uVar6 = uVar6 - 1) {
        *pcVar9 = *pcVar8;
        pcVar8 = pcVar8 + 1;
        pcVar9 = pcVar9 + 1;
      }
      ___crtMessageBoxA(local_1a4,"Microsoft Visual C++ Runtime Library",0x12010);
      return;
    }
  }
  return;
}



// Library Function - Single Match
//  __callnewh
// 
// Library: Visual Studio 1998 Release

int __cdecl __callnewh(size_t _Size)

{
  int iVar1;
  
  if (DAT_00452864 != (code *)0x0) {
    iVar1 = (*DAT_00452864)(_Size);
    if (iVar1 != 0) {
      return 1;
    }
  }
  return 0;
}



// Library Function - Single Match
//  ___sbh_new_region
// 
// Library: Visual Studio 1998 Release

undefined ** ___sbh_new_region(void)

{
  undefined4 *lpAddress;
  LPVOID pvVar1;
  int iVar2;
  int iVar3;
  undefined **lpMem;
  undefined4 *puVar4;
  
  if (DAT_0043b660 == 0) {
    lpMem = &PTR_LOOP_0043ae50;
  }
  else {
    lpMem = (undefined **)HeapAlloc(DAT_00454574,0,0x814);
    if (lpMem == (undefined **)0x0) {
      return (undefined **)0x0;
    }
  }
  lpAddress = (undefined4 *)VirtualAlloc((LPVOID)0x0,0x400000,0x2000,4);
  if (lpAddress != (undefined4 *)0x0) {
    pvVar1 = VirtualAlloc(lpAddress,0x10000,0x1000,4);
    if (pvVar1 != (LPVOID)0x0) {
      if (lpMem == &PTR_LOOP_0043ae50) {
        if (PTR_LOOP_0043ae50 == (undefined *)0x0) {
          PTR_LOOP_0043ae50 = (undefined *)&PTR_LOOP_0043ae50;
        }
        if (PTR_LOOP_0043ae54 == (undefined *)0x0) {
          PTR_LOOP_0043ae54 = (undefined *)&PTR_LOOP_0043ae50;
        }
      }
      else {
        *lpMem = (undefined *)&PTR_LOOP_0043ae50;
        lpMem[1] = PTR_LOOP_0043ae54;
        PTR_LOOP_0043ae54 = (undefined *)lpMem;
        *(undefined ***)lpMem[1] = lpMem;
      }
      lpMem[0x204] = (undefined *)lpAddress;
      lpMem[2] = (undefined *)0x0;
      lpMem[3] = (undefined *)0x10;
      iVar2 = 0;
      do {
        if (iVar2 < 0x10) {
          *(undefined1 *)((int)lpMem + iVar2 + 0x10) = 0xf0;
        }
        else {
          *(undefined1 *)((int)lpMem + iVar2 + 0x10) = 0xff;
        }
        iVar3 = iVar2 + 1;
        *(undefined1 *)((int)lpMem + iVar2 + 0x410) = 0xf1;
        iVar2 = iVar3;
      } while (iVar3 < 0x400);
      puVar4 = lpAddress;
      for (iVar2 = 0x4000; iVar2 != 0; iVar2 = iVar2 + -1) {
        *puVar4 = 0;
        puVar4 = puVar4 + 1;
      }
      if (lpAddress < lpMem[0x204] + 0x10000) {
        do {
          *lpAddress = lpAddress + 2;
          lpAddress[1] = 0xf0;
          *(undefined1 *)(lpAddress + 0x3e) = 0xff;
          lpAddress = lpAddress + 0x400;
        } while (lpAddress < lpMem[0x204] + 0x10000);
      }
      return lpMem;
    }
    VirtualFree(lpAddress,0,0x8000);
  }
  if (lpMem != &PTR_LOOP_0043ae50) {
    HeapFree(DAT_00454574,0,lpMem);
  }
  return (undefined **)0x0;
}



// Library Function - Single Match
//  ___sbh_release_region
// 
// Library: Visual Studio 1998 Release

void __cdecl ___sbh_release_region(undefined **param_1)

{
  VirtualFree(param_1[0x204],0,0x8000);
  if ((undefined **)PTR_LOOP_0043b664 == param_1) {
    PTR_LOOP_0043b664 = param_1[1];
  }
  if (param_1 != &PTR_LOOP_0043ae50) {
    *(undefined **)param_1[1] = *param_1;
    *(undefined **)(*param_1 + 4) = param_1[1];
    HeapFree(DAT_00454574,0,param_1);
    return;
  }
  DAT_0043b660 = 0;
  return;
}



// Library Function - Single Match
//  ___sbh_decommit_pages
// 
// Library: Visual Studio 1998 Release

void __cdecl ___sbh_decommit_pages(int param_1)

{
  BOOL BVar1;
  char *pcVar2;
  undefined *puVar3;
  undefined **ppuVar4;
  undefined **ppuVar5;
  int iVar6;
  int local_4;
  
  ppuVar4 = (undefined **)PTR_LOOP_0043ae54;
  do {
    ppuVar5 = ppuVar4;
    if (ppuVar4[0x204] != (undefined *)0x0) {
      puVar3 = (undefined *)0x3ff;
      pcVar2 = (char *)((int)ppuVar4 + 0x40f);
      local_4 = 0;
      iVar6 = 0x3ff000;
      do {
        if (*pcVar2 == -0x10) {
          BVar1 = VirtualFree(ppuVar4[0x204] + iVar6,0x1000,0x4000);
          if (BVar1 != 0) {
            *pcVar2 = -1;
            DAT_0043b668 = DAT_0043b668 + -1;
            if ((ppuVar4[3] == (undefined *)0xffffffff) || ((int)puVar3 < (int)ppuVar4[3])) {
              ppuVar4[3] = puVar3;
            }
            local_4 = local_4 + 1;
            param_1 = param_1 + -1;
            if (param_1 == 0) break;
          }
        }
        iVar6 = iVar6 + -0x1000;
        puVar3 = puVar3 + -1;
        pcVar2 = pcVar2 + -1;
      } while (-1 < iVar6);
      ppuVar5 = (undefined **)ppuVar4[1];
      if ((local_4 != 0) && (*(char *)(ppuVar4 + 4) == -1)) {
        iVar6 = 1;
        pcVar2 = (char *)((int)ppuVar4 + 0x11);
        do {
          if (*pcVar2 != -1) break;
          iVar6 = iVar6 + 1;
          pcVar2 = pcVar2 + 1;
        } while (iVar6 < 0x400);
        if (iVar6 == 0x400) {
          ___sbh_release_region(ppuVar4);
        }
      }
    }
    if ((ppuVar5 == (undefined **)PTR_LOOP_0043ae54) || (ppuVar4 = ppuVar5, param_1 < 1)) {
      return;
    }
  } while( true );
}



// Library Function - Single Match
//  ___sbh_find_block
// 
// Library: Visual Studio 1998 Release

int __cdecl ___sbh_find_block(undefined *param_1,undefined4 *param_2,uint *param_3)

{
  undefined *puVar1;
  undefined **ppuVar2;
  uint uVar3;
  
  ppuVar2 = &PTR_LOOP_0043ae50;
  while (((puVar1 = ppuVar2[0x204], puVar1 == (undefined *)0x0 || (param_1 <= puVar1)) ||
         (puVar1 + 0x400000 <= param_1))) {
    ppuVar2 = (undefined **)*ppuVar2;
    if (ppuVar2 == &PTR_LOOP_0043ae50) {
      return 0;
    }
  }
  *param_2 = ppuVar2;
  uVar3 = (uint)param_1 & 0xfffff000;
  *param_3 = uVar3;
  return ((int)(param_1 + (-0x100 - uVar3)) >> 4) + 8 + uVar3;
}



// Library Function - Single Match
//  ___sbh_free_block
// 
// Library: Visual Studio 1998 Release

void __cdecl ___sbh_free_block(int param_1,int param_2,char *param_3)

{
  int iVar1;
  
  iVar1 = (param_2 - *(int *)(param_1 + 0x810) >> 0xc) + param_1;
  *(char *)(iVar1 + 0x10) = *(char *)(iVar1 + 0x10) + *param_3;
  *param_3 = '\0';
  *(undefined1 *)(iVar1 + 0x410) = 0xf1;
  if ((*(char *)(iVar1 + 0x10) == -0x10) && (DAT_0043b668 = DAT_0043b668 + 1, DAT_0043b668 == 0x20))
  {
    ___sbh_decommit_pages(0x10);
  }
  return;
}



// Library Function - Single Match
//  ___sbh_alloc_block
// 
// Library: Visual Studio 1998 Release

undefined * __cdecl ___sbh_alloc_block(uint param_1)

{
  char *pcVar1;
  byte bVar2;
  undefined *puVar3;
  undefined *puVar4;
  undefined *puVar5;
  undefined *puVar6;
  undefined4 *puVar7;
  char cVar8;
  int iVar9;
  int iVar10;
  undefined **ppuVar11;
  int *piVar12;
  undefined *puVar13;
  
  piVar12 = (int *)PTR_LOOP_0043b664;
  do {
    cVar8 = (char)param_1;
    if (piVar12[0x204] != 0) {
      iVar10 = piVar12[2];
      if (iVar10 < 0x400) {
        iVar9 = iVar10 << 0xc;
        do {
          bVar2 = *(byte *)((int)piVar12 + iVar10 + 0x10);
          if (((param_1 <= bVar2) && (bVar2 != 0xff)) &&
             (param_1 < *(byte *)((int)piVar12 + iVar10 + 0x410))) {
            puVar6 = (undefined *)
                     ___sbh_alloc_block_from_page
                               ((int *)(piVar12[0x204] + iVar9),(uint)bVar2,param_1);
            if (puVar6 != (undefined *)0x0) {
              pcVar1 = (char *)((int)piVar12 + iVar10 + 0x10);
              PTR_LOOP_0043b664 = (undefined *)piVar12;
              *pcVar1 = *pcVar1 - cVar8;
              piVar12[2] = iVar10;
              return puVar6;
            }
            *(char *)((int)piVar12 + iVar10 + 0x410) = cVar8;
          }
          iVar9 = iVar9 + 0x1000;
          iVar10 = iVar10 + 1;
        } while (iVar9 < 0x400000);
      }
      iVar10 = 0;
      iVar9 = 0;
      if (0 < piVar12[2]) {
        do {
          bVar2 = *(byte *)((int)piVar12 + iVar9 + 0x10);
          if (((param_1 <= bVar2) && (bVar2 != 0xff)) &&
             (param_1 < *(byte *)((int)piVar12 + iVar9 + 0x410))) {
            puVar6 = (undefined *)
                     ___sbh_alloc_block_from_page
                               ((int *)(piVar12[0x204] + iVar10),(uint)bVar2,param_1);
            if (puVar6 != (undefined *)0x0) {
              pcVar1 = (char *)((int)piVar12 + iVar9 + 0x10);
              PTR_LOOP_0043b664 = (undefined *)piVar12;
              *pcVar1 = *pcVar1 - cVar8;
              piVar12[2] = iVar9;
              return puVar6;
            }
            *(char *)((int)piVar12 + iVar9 + 0x410) = cVar8;
          }
          iVar10 = iVar10 + 0x1000;
          iVar9 = iVar9 + 1;
        } while (iVar9 < piVar12[2]);
      }
    }
    piVar12 = (int *)*piVar12;
  } while ((int *)PTR_LOOP_0043b664 != piVar12);
  ppuVar11 = &PTR_LOOP_0043ae50;
  while ((ppuVar11[0x204] == (undefined *)0x0 || (ppuVar11[3] == (undefined *)0xffffffff))) {
    ppuVar11 = (undefined **)*ppuVar11;
    if (ppuVar11 == &PTR_LOOP_0043ae50) {
      ppuVar11 = ___sbh_new_region();
      if (ppuVar11 == (undefined **)0x0) {
        return (undefined *)0x0;
      }
      puVar7 = (undefined4 *)ppuVar11[0x204];
      *(char *)(puVar7 + 2) = cVar8;
      PTR_LOOP_0043b664 = (undefined *)ppuVar11;
      *puVar7 = (undefined *)((int)puVar7 + param_1 + 8);
      puVar7[1] = 0xf0 - param_1;
      *(char *)(ppuVar11 + 4) = *(char *)(ppuVar11 + 4) - cVar8;
      return ppuVar11[0x204] + 0x100;
    }
  }
  puVar3 = ppuVar11[3];
  puVar6 = puVar3 + 0x10;
  puVar5 = puVar3;
  if (0x3ff < (int)puVar6) {
    puVar6 = (undefined *)0x400;
  }
  do {
    puVar13 = puVar5 + 1;
    if ((int)puVar6 <= (int)puVar13) break;
    puVar4 = puVar5 + 0x11;
    puVar5 = puVar13;
  } while (*(char *)((int)ppuVar11 + (int)puVar4) == -1);
  puVar6 = (undefined *)
           VirtualAlloc(ppuVar11[0x204] + (int)puVar3 * 0x1000,((int)puVar13 - (int)puVar3) * 0x1000
                        ,0x1000,4);
  if (puVar6 != ppuVar11[0x204] + (int)puVar3 * 0x1000) {
    return (undefined *)0x0;
  }
  puVar6 = ppuVar11[3];
  piVar12 = (int *)(ppuVar11[0x204] + (int)puVar6 * 0x1000);
  for (; (int)puVar6 < (int)puVar13; puVar6 = puVar6 + 1) {
    *piVar12 = (int)(piVar12 + 2);
    piVar12[1] = 0xf0;
    *(undefined1 *)(piVar12 + 0x3e) = 0xff;
    *(undefined1 *)((int)ppuVar11 + (int)(puVar6 + 0x10)) = 0xf0;
    *(undefined1 *)((int)ppuVar11 + (int)(puVar6 + 0x410)) = 0xf1;
    piVar12 = piVar12 + 0x400;
  }
  for (; ((int)puVar13 < 0x400 && (*(char *)((int)ppuVar11 + (int)(puVar13 + 0x10)) != -1));
      puVar13 = puVar13 + 1) {
  }
  puVar6 = ppuVar11[3];
  PTR_LOOP_0043b664 = (undefined *)ppuVar11;
  ppuVar11[3] = (undefined *)0xffffffff;
  if ((int)puVar13 < 0x400) {
    ppuVar11[3] = puVar13;
  }
  puVar7 = (undefined4 *)(ppuVar11[0x204] + (int)puVar6 * 0x1000);
  *(char *)(puVar7 + 2) = cVar8;
  ppuVar11[2] = puVar6;
  *(char *)((int)ppuVar11 + (int)(puVar6 + 0x10)) =
       *(char *)((int)ppuVar11 + (int)(puVar6 + 0x10)) - cVar8;
  *puVar7 = (undefined *)((int)puVar7 + param_1 + 8);
  puVar7[1] = puVar7[1] - param_1;
  return ppuVar11[0x204] + (int)puVar6 * 0x1000 + 0x100;
}



// Library Function - Single Match
//  ___sbh_alloc_block_from_page
// 
// Library: Visual Studio 1998 Release

int __cdecl ___sbh_alloc_block_from_page(int *param_1,uint param_2,uint param_3)

{
  byte bVar1;
  byte *pbVar2;
  byte *pbVar3;
  byte bVar4;
  uint uVar5;
  byte *pbVar6;
  
  pbVar2 = (byte *)*param_1;
  bVar4 = (byte)param_3;
  if (param_3 <= (uint)param_1[1]) {
    *pbVar2 = bVar4;
    if (pbVar2 + param_3 < param_1 + 0x3e) {
      *param_1 = *param_1 + param_3;
      param_1[1] = param_1[1] - param_3;
    }
    else {
      param_1[1] = 0;
      *param_1 = (int)(param_1 + 2);
    }
    return (int)pbVar2 * 0x10 + (int)param_1 * -0xf + 0x80;
  }
  pbVar6 = pbVar2;
  if (pbVar2[param_1[1]] != 0) {
    pbVar6 = pbVar2 + param_1[1];
  }
  if (pbVar6 + param_3 < param_1 + 0x3e) {
    do {
      if (*pbVar6 == 0) {
        pbVar3 = pbVar6 + 1;
        uVar5 = 1;
        bVar1 = *pbVar3;
        while (bVar1 == 0) {
          pbVar3 = pbVar3 + 1;
          uVar5 = uVar5 + 1;
          bVar1 = *pbVar3;
        }
        if (param_3 <= uVar5) {
          if (pbVar6 + param_3 < param_1 + 0x3e) {
            *param_1 = (int)(pbVar6 + param_3);
            param_1[1] = uVar5 - param_3;
          }
          else {
            param_1[1] = 0;
            *param_1 = (int)(param_1 + 2);
          }
          *pbVar6 = bVar4;
          return (int)pbVar6 * 0x10 + (int)param_1 * -0xf + 0x80;
        }
        if (pbVar2 == pbVar6) {
          param_1[1] = uVar5;
        }
        else {
          param_2 = param_2 - uVar5;
          if (param_2 < param_3) {
            return 0;
          }
        }
      }
      else {
        pbVar3 = pbVar6 + *pbVar6;
      }
      pbVar6 = pbVar3;
    } while (pbVar3 + param_3 < param_1 + 0x3e);
  }
  pbVar6 = (byte *)(param_1 + 2);
  while( true ) {
    while( true ) {
      if ((pbVar2 <= pbVar6) || ((byte *)((int)param_1 + 0xf7U) < pbVar6 + param_3)) {
        return 0;
      }
      if (*pbVar6 == 0) break;
      pbVar6 = pbVar6 + *pbVar6;
    }
    pbVar3 = pbVar6 + 1;
    uVar5 = 1;
    bVar1 = *pbVar3;
    while (bVar1 == 0) {
      pbVar3 = pbVar3 + 1;
      uVar5 = uVar5 + 1;
      bVar1 = *pbVar3;
    }
    if (param_3 <= uVar5) break;
    param_2 = param_2 - uVar5;
    pbVar6 = pbVar3;
    if (param_2 < param_3) {
      return 0;
    }
  }
  if (pbVar6 + param_3 < param_1 + 0x3e) {
    *param_1 = (int)(pbVar6 + param_3);
    param_1[1] = uVar5 - param_3;
  }
  else {
    param_1[1] = 0;
    *param_1 = (int)(param_1 + 2);
  }
  *pbVar6 = bVar4;
  return (int)pbVar6 * 0x10 + (int)param_1 * -0xf + 0x80;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __close
// 
// Library: Visual Studio 1998 Release

int __cdecl __close(int _FileHandle)

{
  int *piVar1;
  int iVar2;
  intptr_t iVar3;
  intptr_t iVar4;
  HANDLE hObject;
  BOOL BVar5;
  DWORD DVar6;
  
  if (DAT_00454680 <= (uint)_FileHandle) {
    _DAT_0043aa58 = 9;
    DAT_0043aa5c = 0;
    return -1;
  }
  piVar1 = (int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3));
  iVar2 = (_FileHandle & 0x1fU) * 8;
  if ((*(byte *)(*piVar1 + 4 + iVar2) & 1) == 0) {
    _DAT_0043aa58 = 9;
    DAT_0043aa5c = 0;
    return -1;
  }
  if ((_FileHandle == 1) || (_FileHandle == 2)) {
    iVar3 = __get_osfhandle(2);
    iVar4 = __get_osfhandle(1);
    if (iVar3 != iVar4) goto LAB_004137ab;
  }
  else {
LAB_004137ab:
    hObject = (HANDLE)__get_osfhandle(_FileHandle);
    BVar5 = CloseHandle(hObject);
    if (BVar5 == 0) {
      DVar6 = GetLastError();
      goto LAB_004137cb;
    }
  }
  DVar6 = 0;
LAB_004137cb:
  __free_osfhnd(_FileHandle);
  if (DVar6 == 0) {
    *(undefined1 *)(*piVar1 + 4 + iVar2) = 0;
    return 0;
  }
  __dosmaperr(DVar6);
  return -1;
}



// Library Function - Single Match
//  __freebuf
// 
// Library: Visual Studio 1998 Release

void __cdecl __freebuf(FILE *_File)

{
  if (((_File->_flag & 0x83U) != 0) && ((_File->_flag & 8U) != 0)) {
    _free(_File->_base);
    _File->_ptr = (char *)0x0;
    _File->_flag = _File->_flag & 0xfffffbf7;
    _File->_base = (char *)0x0;
    _File->_cnt = 0;
  }
  return;
}



// Library Function - Single Match
//  _fflush
// 
// Library: Visual Studio 1998 Release

int __cdecl _fflush(FILE *_File)

{
  int iVar1;
  
  if (_File == (FILE *)0x0) {
    iVar1 = flsall(0);
    return iVar1;
  }
  iVar1 = __flush(_File);
  if (iVar1 != 0) {
    return -1;
  }
  if ((_File->_flag & 0x4000) != 0) {
    iVar1 = __commit(_File->_file);
    return (iVar1 == 0) - 1;
  }
  return 0;
}



// Library Function - Single Match
//  __flush
// 
// Library: Visual Studio 1998 Release

int __cdecl __flush(FILE *_File)

{
  uint uVar1;
  uint _MaxCharCount;
  int iVar2;
  
  iVar2 = 0;
  if ((((byte)_File->_flag & 3) == 2) && ((_File->_flag & 0x108U) != 0)) {
    _MaxCharCount = (int)_File->_ptr - (int)_File->_base;
    if (0 < (int)_MaxCharCount) {
      uVar1 = __write(_File->_file,_File->_base,_MaxCharCount);
      if (uVar1 == _MaxCharCount) {
        if ((_File->_flag & 0x80U) != 0) {
          _File->_flag = _File->_flag & 0xfffffffd;
        }
      }
      else {
        _File->_flag = _File->_flag | 0x20;
        iVar2 = -1;
      }
    }
  }
  _File->_ptr = _File->_base;
  _File->_cnt = 0;
  return iVar2;
}



// Library Function - Single Match
//  _flsall
// 
// Library: Visual Studio 1998 Release

int __cdecl flsall(int param_1)

{
  FILE *_File;
  int iVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  int local_4;
  
  iVar2 = 0;
  iVar4 = 0;
  local_4 = 0;
  if (0 < DAT_00454570) {
    iVar3 = 0;
    do {
      _File = *(FILE **)(DAT_00453564 + iVar3);
      if ((_File != (FILE *)0x0) && ((_File->_flag & 0x83U) != 0)) {
        if (param_1 == 1) {
          iVar1 = _fflush(_File);
          if (iVar1 != -1) {
            iVar2 = iVar2 + 1;
          }
        }
        else if (((param_1 == 0) && ((_File->_flag & 2U) != 0)) &&
                (iVar1 = _fflush(_File), iVar1 == -1)) {
          local_4 = -1;
        }
      }
      iVar3 = iVar3 + 4;
      iVar4 = iVar4 + 1;
    } while (iVar4 < DAT_00454570);
  }
  if (param_1 != 1) {
    iVar2 = local_4;
  }
  return iVar2;
}



// Library Function - Single Match
//  __filbuf
// 
// Library: Visual Studio 1998 Release

int __cdecl __filbuf(FILE *_File)

{
  uint uVar1;
  byte *pbVar2;
  int iVar3;
  undefined *puVar4;
  
  uVar1 = _File->_flag;
  if (((uVar1 & 0x83) == 0) || ((uVar1 & 0x40) != 0)) {
    return -1;
  }
  if ((uVar1 & 2) != 0) {
    _File->_flag = uVar1 | 0x20;
    return -1;
  }
  _File->_flag = uVar1 | 1;
  if ((uVar1 & 0x10c) == 0) {
    __getbuf(_File);
  }
  else {
    _File->_ptr = _File->_base;
  }
  iVar3 = __read(_File->_file,_File->_base,_File->_bufsiz);
  _File->_cnt = iVar3;
  if ((iVar3 != 0) && (iVar3 != -1)) {
    if ((_File->_flag & 0x82U) == 0) {
      uVar1 = _File->_file;
      puVar4 = &DAT_0043ada8;
      if (uVar1 != 0xffffffff) {
        puVar4 = (undefined *)
                 (*(int *)((int)&DAT_00454580 + ((int)(uVar1 & 0xffffffe7) >> 3)) +
                 (uVar1 & 0x1f) * 8);
      }
      if ((puVar4[4] & 0x82) == 0x82) {
        _File->_flag = _File->_flag | 0x2000;
      }
    }
    if (((_File->_bufsiz == 0x200) && ((_File->_flag & 8U) != 0)) && ((_File->_flag & 0x400U) == 0))
    {
      _File->_bufsiz = 0x1000;
    }
    _File->_cnt = _File->_cnt + -1;
    pbVar2 = (byte *)_File->_ptr;
    _File->_ptr = (char *)(pbVar2 + 1);
    return (uint)*pbVar2;
  }
  _File->_flag = _File->_flag | (-(uint)(iVar3 == 0) & 0xfffffff0) + 0x20;
  _File->_cnt = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __read
// 
// Library: Visual Studio 1998 Release

int __cdecl __read(int _FileHandle,void *_DstBuf,uint _MaxCharCount)

{
  int *piVar1;
  int iVar2;
  byte *pbVar3;
  char cVar4;
  int iVar5;
  BOOL BVar6;
  DWORD DVar7;
  byte bVar8;
  void *lpBuffer;
  char *pcVar9;
  char *pcVar10;
  char *pcVar11;
  char cStack_d;
  DWORD local_c;
  DWORD local_8;
  char *pcStack_4;
  
  if ((uint)_FileHandle < DAT_00454680) {
    piVar1 = (int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3));
    iVar2 = (_FileHandle & 0x1fU) * 8;
    iVar5 = *piVar1 + iVar2;
    if ((*(byte *)(iVar5 + 4) & 1) != 0) {
      local_c = 0;
      if ((_MaxCharCount == 0) || ((*(byte *)(iVar5 + 4) & 2) != 0)) {
        return 0;
      }
      lpBuffer = _DstBuf;
      if (((*(byte *)(iVar5 + 4) & 0x48) != 0) && (*(char *)(iVar5 + 5) != '\n')) {
        *(char *)_DstBuf = *(char *)(iVar5 + 5);
        lpBuffer = (void *)((int)_DstBuf + 1);
        _MaxCharCount = _MaxCharCount - 1;
        local_c = 1;
        *(undefined1 *)(*piVar1 + 5 + iVar2) = 10;
      }
      BVar6 = ReadFile(*(HANDLE *)(*piVar1 + iVar2),lpBuffer,_MaxCharCount,&local_8,
                       (LPOVERLAPPED)0x0);
      if (BVar6 == 0) {
        DVar7 = GetLastError();
        if (DVar7 == 5) {
          DAT_0043aa5c = DVar7;
          _DAT_0043aa58 = 9;
          return -1;
        }
        if (DVar7 != 0x6d) {
          __dosmaperr(DVar7);
          return -1;
        }
        return 0;
      }
      local_c = local_c + local_8;
      pbVar3 = (byte *)(*piVar1 + 4 + iVar2);
      bVar8 = *pbVar3;
      if ((bVar8 & 0x80) != 0) {
                    // WARNING: Load size is inaccurate
        if ((local_8 == 0) || (*_DstBuf != '\n')) {
          bVar8 = bVar8 & 0xfb;
        }
        else {
          bVar8 = bVar8 | 4;
        }
        *pbVar3 = bVar8;
        pcStack_4 = (char *)(local_c + (int)_DstBuf);
        pcVar9 = (char *)_DstBuf;
        pcVar11 = (char *)_DstBuf;
        if (_DstBuf < pcStack_4) {
          do {
            cVar4 = *pcVar9;
            if (cVar4 == '\x1a') {
              pbVar3 = (byte *)(*piVar1 + 4 + iVar2);
              bVar8 = *pbVar3;
              if ((bVar8 & 0x40) == 0) {
                *pbVar3 = bVar8 | 2;
              }
              break;
            }
            if (cVar4 == '\r') {
              if (pcVar9 < pcStack_4 + -1) {
                pcVar10 = pcVar9 + 1;
                if (*pcVar10 == '\n') {
                  pcVar10 = pcVar9 + 2;
                  *pcVar11 = '\n';
                }
                else {
                  *pcVar11 = '\r';
                }
                goto LAB_00413cb4;
              }
              pcVar10 = pcVar9 + 1;
              local_c = 0;
              BVar6 = ReadFile(*(HANDLE *)(*piVar1 + iVar2),&cStack_d,1,&local_8,(LPOVERLAPPED)0x0);
              if (BVar6 == 0) {
                local_c = GetLastError();
              }
              if ((local_c != 0) || (local_8 == 0)) {
LAB_00413cb1:
                *pcVar11 = '\r';
                goto LAB_00413cb4;
              }
              if ((*(byte *)(*piVar1 + 4 + iVar2) & 0x48) == 0) {
                if ((pcVar11 == (char *)_DstBuf) && (cStack_d == '\n')) {
                  *pcVar11 = '\n';
                  goto LAB_00413cb4;
                }
                __lseek(_FileHandle,-1,1);
                if (cStack_d != '\n') goto LAB_00413cb1;
              }
              else {
                if (cStack_d == '\n') {
                  *pcVar11 = '\n';
                  goto LAB_00413cb4;
                }
                *pcVar11 = '\r';
                pcVar11 = pcVar11 + 1;
                *(char *)(*piVar1 + 5 + iVar2) = cStack_d;
              }
            }
            else {
              pcVar10 = pcVar9 + 1;
              *pcVar11 = cVar4;
LAB_00413cb4:
              pcVar11 = pcVar11 + 1;
            }
            pcVar9 = pcVar10;
          } while (pcVar10 < pcStack_4);
        }
        local_c = (int)pcVar11 - (int)_DstBuf;
      }
      return local_c;
    }
  }
  _DAT_0043aa58 = 9;
  DAT_0043aa5c = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __fseeki64
// 
// Library: Visual Studio 1998 Release

int __cdecl __fseeki64(FILE *_File,longlong _Offset,int _Origin)

{
  uint uVar1;
  int unaff_EDI;
  longlong lVar2;
  undefined4 in_stack_00000008;
  
  if (((_File->_flag & 0x83U) == 0) ||
     (((_Offset._4_4_ != 0 && (_Offset._4_4_ != 1)) && (_Offset._4_4_ != 2)))) {
    _DAT_0043aa58 = 0x16;
    return -1;
  }
  _File->_flag = _File->_flag & 0xffffffef;
  lVar2 = _Offset;
  if (_Offset._4_4_ == 1) {
    lVar2 = __ftelli64(_File);
    lVar2 = lVar2 + CONCAT44((undefined4)_Offset,in_stack_00000008);
    in_stack_00000008 = (undefined4)lVar2;
    lVar2 = CONCAT44(1,(int)((ulonglong)lVar2 >> 0x20));
    _Offset._4_4_ = 0;
  }
  __flush(_File);
  _Offset._0_4_ = (undefined4)lVar2;
  uVar1 = _File->_flag;
  if ((uVar1 & 0x80) == 0) {
    if ((((uVar1 & 1) != 0) && ((uVar1 & 8) != 0)) && ((uVar1 & 0x400) == 0)) {
      _File->_bufsiz = 0x200;
    }
  }
  else {
    _File->_flag = uVar1 & 0xfffffffc;
  }
  lVar2 = __lseeki64(_File->_file,CONCAT44(_Offset._4_4_,(undefined4)_Offset),unaff_EDI);
  if (lVar2 != -1) {
    return 0;
  }
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __openfile
// 
// Library: Visual Studio 1998 Release

FILE * __cdecl __openfile(char *_Filename,char *_Mode,int _ShFlag,FILE *_File)

{
  char cVar1;
  bool bVar2;
  bool bVar3;
  bool bVar4;
  int iVar5;
  char *pcVar6;
  uint _OpenFlag;
  uint uVar7;
  
  bVar3 = false;
  cVar1 = *_Mode;
  bVar4 = false;
  if (cVar1 == 'a') {
    _OpenFlag = 0x109;
  }
  else {
    if (cVar1 == 'r') {
      _OpenFlag = 0;
      uVar7 = DAT_0043be00 | 1;
      goto LAB_00413e1d;
    }
    if (cVar1 != 'w') {
      return (FILE *)0x0;
    }
    _OpenFlag = 0x301;
  }
  uVar7 = DAT_0043be00 | 2;
LAB_00413e1d:
  bVar2 = true;
  pcVar6 = _Mode + 1;
  cVar1 = *pcVar6;
  while ((cVar1 != '\0' && (bVar2))) {
    switch(*pcVar6) {
    case '+':
      if ((_OpenFlag & 2) == 0) {
        _OpenFlag = _OpenFlag & 0xfffffffe | 2;
        uVar7 = uVar7 & 0xfffffffc | 0x80;
      }
      else {
        bVar2 = false;
      }
      break;
    default:
      bVar2 = false;
      break;
    case 'D':
      if ((_OpenFlag & 0x40) == 0) {
        _OpenFlag = _OpenFlag | 0x40;
      }
      else {
        bVar2 = false;
      }
      break;
    case 'R':
      if (bVar3) {
        bVar2 = false;
      }
      else {
        bVar3 = true;
        _OpenFlag = _OpenFlag | 0x10;
      }
      break;
    case 'S':
      if (bVar3) {
        bVar2 = false;
      }
      else {
        bVar3 = true;
        _OpenFlag = _OpenFlag | 0x20;
      }
      break;
    case 'T':
      if ((_OpenFlag & 0x1000) == 0) {
        _OpenFlag = _OpenFlag | 0x1000;
      }
      else {
        bVar2 = false;
      }
      break;
    case 'b':
      if ((_OpenFlag & 0xc000) == 0) {
        _OpenFlag = _OpenFlag | 0x8000;
      }
      else {
        bVar2 = false;
      }
      break;
    case 'c':
      if (bVar4) {
        bVar2 = false;
      }
      else {
        bVar4 = true;
        uVar7 = uVar7 | 0x4000;
      }
      break;
    case 'n':
      if (bVar4) {
        bVar2 = false;
      }
      else {
        bVar4 = true;
        uVar7 = uVar7 & 0xffffbfff;
      }
      break;
    case 't':
      if ((_OpenFlag & 0xc000) == 0) {
        _OpenFlag = _OpenFlag | 0x4000;
      }
      else {
        bVar2 = false;
      }
    }
    pcVar6 = pcVar6 + 1;
    cVar1 = *pcVar6;
  }
  iVar5 = __sopen(_Filename,_OpenFlag,_ShFlag,0x1a4);
  if (-1 < iVar5) {
    _DAT_0043ba68 = _DAT_0043ba68 + 1;
    _File->_flag = uVar7;
    _File->_cnt = 0;
    _File->_ptr = (char *)0x0;
    _File->_base = (char *)0x0;
    _File->_tmpfname = (char *)0x0;
    _File->_file = iVar5;
    return _File;
  }
  return (FILE *)0x0;
}



// Library Function - Single Match
//  __getstream
// 
// Library: Visual Studio 1998 Release

FILE * __cdecl __getstream(void)

{
  void *pvVar1;
  int *piVar2;
  FILE *pFVar3;
  int iVar4;
  
  pFVar3 = (FILE *)0x0;
  iVar4 = 0;
  piVar2 = DAT_00453564;
  if (0 < DAT_00454570) {
    do {
      if (*piVar2 == 0) {
        pvVar1 = _malloc(0x20);
        DAT_00453564[iVar4] = (int)pvVar1;
        if ((FILE *)DAT_00453564[iVar4] != (FILE *)0x0) {
          pFVar3 = (FILE *)DAT_00453564[iVar4];
        }
        break;
      }
      if ((*(uint *)(*piVar2 + 0xc) & 0x83) == 0) {
        pFVar3 = (FILE *)DAT_00453564[iVar4];
        break;
      }
      iVar4 = iVar4 + 1;
      piVar2 = piVar2 + 1;
    } while (iVar4 < DAT_00454570);
  }
  if (pFVar3 != (FILE *)0x0) {
    pFVar3->_cnt = 0;
    pFVar3->_flag = 0;
    pFVar3->_base = (char *)0x0;
    pFVar3->_ptr = (char *)0x0;
    pFVar3->_tmpfname = (char *)0x0;
    pFVar3->_file = -1;
  }
  return pFVar3;
}



// Library Function - Single Match
//  __flsbuf
// 
// Library: Visual Studio 1998 Release

int __cdecl __flsbuf(int _Ch,FILE *_File)

{
  uint _FileHandle;
  FILE *_File_00;
  int iVar1;
  undefined *puVar2;
  uint uVar3;
  uint uVar4;
  
  _File_00 = _File;
  _FileHandle = _File->_file;
  uVar3 = _File->_flag;
  if (((uVar3 & 0x82) == 0) || ((uVar3 & 0x40) != 0)) {
    _File->_flag = uVar3 | 0x20;
    return -1;
  }
  if ((uVar3 & 1) != 0) {
    _File->_cnt = 0;
    if ((_File->_flag & 0x10U) == 0) {
      _File->_flag = _File->_flag | 0x20;
      return -1;
    }
    _File->_ptr = _File->_base;
    _File->_flag = _File->_flag & 0xfffffffe;
  }
  uVar3 = _File->_flag;
  uVar4 = 0;
  _File->_flag = uVar3 | 2;
  _File->_flag = uVar3 & 0xffffffef | 2;
  _File->_cnt = 0;
  if ((_File->_flag & 0x10cU) == 0) {
    if ((_File == (FILE *)&DAT_0043b808) || (_File == (FILE *)&DAT_0043b828)) {
      iVar1 = __isatty(_FileHandle);
      if (iVar1 != 0) goto LAB_004140e4;
    }
    __getbuf(_File_00);
  }
LAB_004140e4:
  if ((_File_00->_flag & 0x108U) == 0) {
    uVar3 = 1;
    uVar4 = __write(_FileHandle,&_Ch,1);
  }
  else {
    uVar3 = (int)_File_00->_ptr - (int)_File_00->_base;
    _File_00->_ptr = _File_00->_base + 1;
    _File_00->_cnt = _File_00->_bufsiz + -1;
    if ((int)uVar3 < 1) {
      puVar2 = &DAT_0043ada8;
      if (_FileHandle != 0xffffffff) {
        puVar2 = (undefined *)
                 (*(int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7) >> 3)) +
                 (_FileHandle & 0x1f) * 8);
      }
      if ((puVar2[4] & 0x20) != 0) {
        __lseek(_FileHandle,0,2);
      }
    }
    else {
      uVar4 = __write(_FileHandle,_File_00->_base,uVar3);
    }
    *_File_00->_base = (char)_Ch;
  }
  if (uVar4 != uVar3) {
    _File_00->_flag = _File_00->_flag | 0x20;
    return -1;
  }
  return _Ch & 0xff;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __write
// 
// Library: Visual Studio 1998 Release

int __cdecl __write(int _FileHandle,void *_Buf,uint _MaxCharCount)

{
  byte bVar1;
  char cVar2;
  BOOL BVar3;
  char *pcVar4;
  int iVar5;
  char *pcVar6;
  ulong local_418;
  DWORD local_414;
  int *local_410;
  int local_40c;
  DWORD local_408;
  char local_404 [1028];
  
  if ((uint)_FileHandle < DAT_00454680) {
    local_410 = (int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3));
    local_40c = (_FileHandle & 0x1fU) * 8;
    bVar1 = *(byte *)(*local_410 + 4 + local_40c);
    if ((bVar1 & 1) != 0) {
      iVar5 = 0;
      local_408 = 0;
      if (_MaxCharCount == 0) {
        return 0;
      }
      if ((bVar1 & 0x20) != 0) {
        __lseek(_FileHandle,0,2);
      }
      if ((*(byte *)((undefined4 *)(local_40c + *local_410) + 1) & 0x80) == 0) {
        BVar3 = WriteFile(*(HANDLE *)(local_40c + *local_410),_Buf,_MaxCharCount,&local_414,
                          (LPOVERLAPPED)0x0);
        if (BVar3 == 0) {
LAB_004142f5:
          local_418 = GetLastError();
        }
        else {
          local_418 = 0;
          local_408 = local_414;
        }
      }
      else {
        local_418 = 0;
        pcVar4 = (char *)_Buf;
        do {
          if (_MaxCharCount <= (uint)((int)pcVar4 - (int)_Buf)) break;
          pcVar6 = local_404;
          do {
            if (_MaxCharCount <= (uint)((int)pcVar4 - (int)_Buf)) break;
            cVar2 = *pcVar4;
            pcVar4 = pcVar4 + 1;
            if (cVar2 == '\n') {
              *pcVar6 = '\r';
              iVar5 = iVar5 + 1;
              pcVar6 = pcVar6 + 1;
            }
            *pcVar6 = cVar2;
            pcVar6 = pcVar6 + 1;
          } while ((int)pcVar6 - (int)local_404 < 0x400);
          BVar3 = WriteFile(*(HANDLE *)(*local_410 + local_40c),local_404,
                            (int)pcVar6 - (int)local_404,&local_414,(LPOVERLAPPED)0x0);
          if (BVar3 == 0) goto LAB_004142f5;
          local_408 = local_408 + local_414;
        } while ((int)pcVar6 - (int)local_404 <= (int)local_414);
      }
      if (local_408 != 0) {
        return local_408 - iVar5;
      }
      if (local_418 == 0) {
                    // WARNING: Load size is inaccurate
        if (((*(byte *)(*local_410 + 4 + local_40c) & 0x40) != 0) && (*_Buf == '\x1a')) {
          return 0;
        }
        _DAT_0043aa58 = 0x1c;
        DAT_0043aa5c = 0;
        return -1;
      }
      if (local_418 != 5) {
        __dosmaperr(local_418);
        return -1;
      }
      _DAT_0043aa58 = 9;
      DAT_0043aa5c = local_418;
      return -1;
    }
  }
  _DAT_0043aa58 = 9;
  DAT_0043aa5c = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __lseek
// 
// Library: Visual Studio 1998 Release

long __cdecl __lseek(int _FileHandle,long _Offset,int _Origin)

{
  int *piVar1;
  int iVar2;
  byte *pbVar3;
  HANDLE hFile;
  DWORD DVar4;
  ulong uVar5;
  
  if ((uint)_FileHandle < DAT_00454680) {
    piVar1 = (int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3));
    iVar2 = (_FileHandle & 0x1fU) * 8;
    if ((*(byte *)(*piVar1 + 4 + iVar2) & 1) != 0) {
      hFile = (HANDLE)__get_osfhandle(_FileHandle);
      if (hFile == (HANDLE)0xffffffff) {
        _DAT_0043aa58 = 9;
        return -1;
      }
      DVar4 = SetFilePointer(hFile,_Offset,(PLONG)0x0,_Origin);
      uVar5 = 0;
      if (DVar4 == 0xffffffff) {
        uVar5 = GetLastError();
      }
      if (uVar5 != 0) {
        __dosmaperr(uVar5);
        return -1;
      }
      pbVar3 = (byte *)(*piVar1 + 4 + iVar2);
      *pbVar3 = *pbVar3 & 0xfd;
      return DVar4;
    }
  }
  _DAT_0043aa58 = 9;
  DAT_0043aa5c = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __dosmaperr
// 
// Library: Visual Studio 1998 Release

void __cdecl __dosmaperr(ulong param_1)

{
  int iVar1;
  ulong *puVar2;
  
  iVar1 = 0;
  puVar2 = &DAT_0043b670;
  DAT_0043aa5c = param_1;
  do {
    if (*puVar2 == param_1) {
      _DAT_0043aa58 = (&DAT_0043b674)[iVar1 * 2];
      return;
    }
    puVar2 = puVar2 + 2;
    iVar1 = iVar1 + 1;
  } while (puVar2 < &DAT_0043b7d8);
  if ((0x12 < param_1) && (param_1 < 0x25)) {
    _DAT_0043aa58 = 0xd;
    return;
  }
  if ((0xbb < param_1) && (param_1 < 0xcb)) {
    _DAT_0043aa58 = 8;
    return;
  }
  _DAT_0043aa58 = 0x16;
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __stbuf
// 
// Library: Visual Studio 1998 Release

int __cdecl __stbuf(FILE *_File)

{
  int *piVar1;
  char *pcVar2;
  int iVar3;
  void *pvVar4;
  
  iVar3 = __isatty(_File->_file);
  if (iVar3 == 0) {
    return 0;
  }
  if (_File == (FILE *)&DAT_0043b808) {
    iVar3 = 0;
  }
  else {
    if (_File != (FILE *)&DAT_0043b828) {
      return 0;
    }
    iVar3 = 1;
  }
  _DAT_0043ba68 = _DAT_0043ba68 + 1;
  if ((_File->_flag & 0x10cU) != 0) {
    return 0;
  }
  piVar1 = &DAT_0043b7e0 + iVar3;
  if (*piVar1 == 0) {
    pvVar4 = _malloc(0x1000);
    *piVar1 = (int)pvVar4;
    if (pvVar4 == (void *)0x0) {
      return 0;
    }
  }
  pcVar2 = (char *)*piVar1;
  _File->_base = pcVar2;
  _File->_ptr = pcVar2;
  _File->_bufsiz = 0x1000;
  _File->_cnt = 0x1000;
  _File->_flag = _File->_flag | 0x1102;
  return 1;
}



// Library Function - Single Match
//  __ftbuf
// 
// Library: Visual Studio 1998 Release

void __cdecl __ftbuf(int _Flag,FILE *_File)

{
  if (_Flag == 0) {
    if ((_File->_flag & 0x1000) != 0) {
      __flush(_File);
    }
  }
  else if ((_File->_flag & 0x1000) != 0) {
    __flush(_File);
    _File->_flag = _File->_flag & 0xffffeeff;
    _File->_bufsiz = 0;
    _File->_ptr = (char *)0x0;
    _File->_base = (char *)0x0;
    return;
  }
  return;
}



// Library Function - Single Match
//  __output
// 
// Library: Visual Studio 1998 Release

int __cdecl __output(FILE *param_1,byte *param_2,undefined4 *param_3)

{
  char cVar1;
  wchar_t wVar2;
  uint uVar3;
  undefined1 *puVar4;
  short *psVar5;
  int *piVar6;
  undefined4 uVar7;
  int iVar8;
  byte bVar9;
  undefined1 *puVar10;
  wchar_t *pwVar11;
  undefined1 *puVar12;
  undefined1 *puVar13;
  char *pcVar14;
  undefined8 uVar15;
  char local_246;
  char local_245;
  char local_244 [4];
  wchar_t *local_240;
  undefined8 local_23c;
  int local_234;
  undefined8 local_230;
  int local_228;
  int local_224;
  int local_220;
  int local_21c;
  uint local_218;
  int local_214;
  int local_210;
  undefined4 local_20c;
  undefined4 local_208;
  undefined4 local_204;
  undefined4 local_200;
  undefined1 local_1;
  
  local_234 = 0;
  bVar9 = *param_2;
  local_21c = 0;
  param_2 = param_2 + 1;
  puVar4 = local_200;
  puVar12 = local_200;
  puVar10 = local_200;
  do {
    if ((bVar9 == 0) || (local_234 < 0)) {
      return local_234;
    }
    if (((char)bVar9 < ' ') || ('x' < (char)bVar9)) {
      uVar3 = 0;
    }
    else {
      uVar3 = (byte)"<program name unknown>"[(char)bVar9 + 0x10] & 0xf;
    }
    local_21c = (int)((char)(&DAT_0041b638)[uVar3 * 8 + local_21c] >> 4);
    switch(local_21c) {
    case 0:
switchD_00414845_caseD_0:
      local_220 = 0;
      if ((PTR_DAT_0043bb90[(uint)bVar9 * 2 + 1] & 0x80) != 0) {
        write_char((int)(char)bVar9,param_1,&local_234);
        bVar9 = *param_2;
        param_2 = param_2 + 1;
      }
      write_char((int)(char)bVar9,param_1,&local_234);
      break;
    case 1:
      local_20c = 0;
      puVar12 = (undefined1 *)0x0;
      puVar4 = (undefined1 *)0xffffffff;
      local_210 = 0;
      local_228 = 0;
      local_224 = 0;
      local_220 = 0;
      break;
    case 2:
      switch(bVar9) {
      case 0x20:
        puVar12 = (undefined1 *)((uint)puVar12 | 2);
        break;
      case 0x23:
        puVar12 = (undefined1 *)((uint)puVar12 | 0x80);
        break;
      case 0x2b:
        puVar12 = (undefined1 *)((uint)puVar12 | 1);
        break;
      case 0x2d:
        puVar12 = (undefined1 *)((uint)puVar12 | 4);
        break;
      case 0x30:
        puVar12 = (undefined1 *)((uint)puVar12 | 8);
      }
      break;
    case 3:
      if (bVar9 == 0x2a) {
        local_228 = get_int_arg((int *)&param_3);
        if (local_228 < 0) {
          local_228 = -local_228;
          puVar12 = (undefined1 *)((uint)puVar12 | 4);
        }
      }
      else {
        local_228 = (char)bVar9 + -0x30 + local_228 * 10;
      }
      break;
    case 4:
      puVar4 = (undefined1 *)0x0;
      break;
    case 5:
      if (bVar9 == 0x2a) {
        puVar4 = (undefined1 *)get_int_arg((int *)&param_3);
        if ((int)puVar4 < 0) {
          puVar4 = (undefined1 *)0xffffffff;
        }
      }
      else {
        puVar4 = (undefined1 *)((char)bVar9 + -0x30 + (int)puVar4 * 10);
      }
      break;
    case 6:
      switch(bVar9) {
      case 0x49:
        if ((*param_2 != 0x36) || (param_2[1] != 0x34)) {
          local_21c = 0;
          goto switchD_00414845_caseD_0;
        }
        param_2 = param_2 + 2;
        puVar12 = (undefined1 *)((uint)puVar12 | 0x8000);
        break;
      case 0x68:
        puVar12 = (undefined1 *)((uint)puVar12 | 0x20);
        break;
      case 0x6c:
        puVar12 = (undefined1 *)((uint)puVar12 | 0x10);
        break;
      case 0x77:
        puVar12 = (undefined1 *)((uint)puVar12 | 0x800);
      }
      break;
    case 7:
      pwVar11 = local_240;
      switch(bVar9) {
      case 0x43:
        if (((uint)puVar12 & 0x830) == 0) {
          puVar12 = (undefined1 *)((uint)puVar12 | 0x800);
        }
      case 99:
        if (((uint)puVar12 & 0x810) == 0) {
          puVar10 = (undefined1 *)0x1;
          uVar7 = get_int_arg((int *)&param_3);
          local_200 = (undefined1 *)CONCAT31(local_200._1_3_,(char)uVar7);
        }
        else {
          wVar2 = get_short_arg(&param_3);
          puVar10 = (undefined1 *)_wctomb((char *)&local_200,wVar2);
          if ((int)puVar10 < 0) {
            local_210 = 1;
          }
        }
        pwVar11 = (wchar_t *)&local_200;
        break;
      case 0x45:
      case 0x47:
        local_20c = 1;
        bVar9 = bVar9 + 0x20;
      case 0x65:
      case 0x66:
      case 0x67:
        puVar13 = (undefined1 *)((uint)puVar12 | 0x40);
        local_240 = (wchar_t *)&local_200;
        if ((int)puVar4 < 0) {
          puVar4 = (undefined1 *)0x6;
        }
        else if ((puVar4 == (undefined1 *)0x0) && (bVar9 == 0x67)) {
          puVar4 = (undefined1 *)0x1;
        }
        local_208 = *param_3;
        local_204 = param_3[1];
        param_3 = param_3 + 2;
        (*(code *)PTR_FUN_0043aac0)(&local_208,&local_200,(int)(char)bVar9,puVar4,local_20c);
        if ((((uint)puVar12 & 0x80) != 0) && (puVar4 == (undefined1 *)0x0)) {
          (*(code *)PTR_FUN_0043aacc)(&local_200);
        }
        if ((bVar9 == 0x67) && (((uint)puVar12 & 0x80) == 0)) {
          (*(code *)PTR_FUN_0043aac4)(&local_200);
        }
        if ((char)local_200 == '-') {
          puVar13 = (undefined1 *)((uint)puVar12 | 0x140);
          local_240 = (wchar_t *)((int)&local_200 + 1);
        }
        uVar3 = 0xffffffff;
        pwVar11 = local_240;
        do {
          if (uVar3 == 0) break;
          uVar3 = uVar3 - 1;
          wVar2 = *pwVar11;
          pwVar11 = (wchar_t *)((int)pwVar11 + 1);
        } while ((char)wVar2 != '\0');
        puVar10 = (undefined1 *)(~uVar3 - 1);
        puVar12 = puVar13;
        pwVar11 = local_240;
        break;
      case 0x53:
        if (((uint)puVar12 & 0x830) == 0) {
          puVar12 = (undefined1 *)((uint)puVar12 | 0x800);
        }
      case 0x73:
        puVar10 = (undefined1 *)0x7fffffff;
        if (puVar4 != (undefined1 *)0xffffffff) {
          puVar10 = puVar4;
        }
        local_240 = (wchar_t *)get_int_arg((int *)&param_3);
        if (((uint)puVar12 & 0x810) == 0) {
          pwVar11 = local_240;
          if (local_240 == (wchar_t *)0x0) {
            local_240 = (wchar_t *)PTR_DAT_0043ba6c;
            pwVar11 = (wchar_t *)PTR_DAT_0043ba6c;
          }
          for (; (puVar10 != (undefined1 *)0x0 && (puVar10 = puVar10 + -1, (char)*pwVar11 != '\0'));
              pwVar11 = (wchar_t *)((int)pwVar11 + 1)) {
          }
          puVar10 = (undefined1 *)((int)pwVar11 - (int)local_240);
          pwVar11 = local_240;
        }
        else {
          if (local_240 == (wchar_t *)0x0) {
            local_240 = (wchar_t *)PTR_DAT_0043ba70;
          }
          local_220 = 1;
          for (pwVar11 = local_240;
              (puVar10 != (undefined1 *)0x0 && (puVar10 = puVar10 + -1, *pwVar11 != L'\0'));
              pwVar11 = pwVar11 + 1) {
          }
          puVar10 = (undefined1 *)((int)pwVar11 - (int)local_240 >> 1);
          pwVar11 = local_240;
        }
        break;
      case 0x5a:
        psVar5 = (short *)get_int_arg((int *)&param_3);
        if ((psVar5 == (short *)0x0) ||
           (pwVar11 = *(wchar_t **)(psVar5 + 2), pwVar11 == (wchar_t *)0x0)) {
          uVar3 = 0xffffffff;
          local_240 = (wchar_t *)PTR_DAT_0043ba6c;
          pcVar14 = PTR_DAT_0043ba6c;
          do {
            if (uVar3 == 0) break;
            uVar3 = uVar3 - 1;
            cVar1 = *pcVar14;
            pcVar14 = pcVar14 + 1;
          } while (cVar1 != '\0');
          puVar10 = (undefined1 *)(~uVar3 - 1);
          pwVar11 = local_240;
        }
        else if (((uint)puVar12 & 0x800) == 0) {
          local_220 = 0;
          puVar10 = (undefined1 *)(int)*psVar5;
        }
        else {
          local_220 = 1;
          puVar10 = (undefined1 *)((uint)(int)*psVar5 >> 1);
        }
        break;
      case 100:
      case 0x69:
        local_218 = 10;
        puVar12 = (undefined1 *)((uint)puVar12 | 0x40);
        goto LAB_00414c27;
      case 0x6e:
        piVar6 = (int *)get_int_arg((int *)&param_3);
        if (((uint)puVar12 & 0x20) == 0) {
          *piVar6 = local_234;
        }
        else {
          *(short *)piVar6 = (short)local_234;
        }
        local_210 = 1;
        pwVar11 = local_240;
        break;
      case 0x6f:
        local_218 = 8;
        if (((uint)puVar12 & 0x80) != 0) {
          puVar12 = (undefined1 *)((uint)puVar12 | 0x200);
        }
        goto LAB_00414c27;
      case 0x70:
        puVar4 = (undefined1 *)0x8;
      case 0x58:
        local_214 = 7;
        goto LAB_00414c00;
      case 0x75:
        local_218 = 10;
        goto LAB_00414c27;
      case 0x78:
        local_214 = 0x27;
LAB_00414c00:
        local_218 = 0x10;
        if (((uint)puVar12 & 0x80) != 0) {
          local_246 = '0';
          local_224 = 2;
          local_245 = (char)local_214 + 'Q';
        }
LAB_00414c27:
        if (((uint)puVar12 & 0x8000) == 0) {
          if (((uint)puVar12 & 0x20) == 0) {
            if (((uint)puVar12 & 0x40) == 0) {
              uVar3 = get_int_arg((int *)&param_3);
              goto LAB_00414cc6;
            }
            iVar8 = get_int_arg((int *)&param_3);
            local_23c = (ulonglong)iVar8;
          }
          else if (((uint)puVar12 & 0x40) == 0) {
            uVar3 = get_int_arg((int *)&param_3);
            uVar3 = uVar3 & 0xffff;
LAB_00414cc6:
            local_23c = (ulonglong)uVar3;
          }
          else {
            uVar7 = get_int_arg((int *)&param_3);
            local_23c = (ulonglong)(int)(short)uVar7;
          }
        }
        else {
          local_23c = get_int64_arg((int *)&param_3);
        }
        if (((((uint)puVar12 & 0x40) == 0) || (0 < local_23c._4_4_)) || (-1 < (longlong)local_23c))
        {
          local_230 = local_23c;
        }
        else {
          puVar12 = (undefined1 *)((uint)puVar12 | 0x100);
          local_230 = CONCAT44(-(local_23c._4_4_ + (uint)((uint)local_23c != 0)),-(uint)local_23c);
        }
        if (((uint)puVar12 & 0x8000) == 0) {
          local_230 = local_230 & 0xffffffff;
        }
        if ((int)puVar4 < 0) {
          puVar4 = (undefined1 *)0x1;
        }
        else {
          puVar12 = (undefined1 *)((uint)puVar12 & 0xfffffff7);
        }
        if ((local_230._4_4_ == 0) && ((uint)local_230 == 0)) {
          local_224 = 0;
        }
        puVar10 = puVar4;
        local_240 = (wchar_t *)&local_1;
        while( true ) {
          puVar4 = puVar10 + -1;
          if ((((int)puVar10 < 1) && (local_230._4_4_ == 0)) && ((uint)local_230 == 0)) break;
          uVar3 = (int)local_218 >> 0x1f;
          local_23c = (ulonglong)(int)local_218;
          uVar15 = __aullrem((uint)local_230,local_230._4_4_,local_218,uVar3);
          iVar8 = (int)uVar15 + 0x30;
          local_230 = __aulldiv((uint)local_230,local_230._4_4_,(uint)local_23c,uVar3);
          if (0x39 < iVar8) {
            iVar8 = iVar8 + local_214;
          }
          *(char *)local_240 = (char)iVar8;
          puVar10 = puVar4;
          local_240 = (wchar_t *)((int)local_240 + -1);
        }
        puVar10 = &local_1 + -(int)local_240;
        pwVar11 = (wchar_t *)((int)local_240 + 1);
        if ((((uint)puVar12 & 0x200) != 0) &&
           ((*(char *)pwVar11 != '0' || (puVar10 == (undefined1 *)0x0)))) {
          puVar10 = &stack0x00000000 + -(int)local_240;
          *(char *)local_240 = '0';
          pwVar11 = local_240;
        }
      }
      local_240 = pwVar11;
      if (local_210 == 0) {
        if (((uint)puVar12 & 0x40) != 0) {
          if (((uint)puVar12 & 0x100) == 0) {
            if (((uint)puVar12 & 1) == 0) {
              if (((uint)puVar12 & 2) == 0) goto LAB_00414f39;
              local_246 = ' ';
            }
            else {
              local_246 = '+';
            }
          }
          else {
            local_246 = '-';
          }
          local_224 = 1;
        }
LAB_00414f39:
        iVar8 = (local_228 - (int)puVar10) - local_224;
        local_230 = CONCAT44(local_230._4_4_,iVar8);
        if (((uint)puVar12 & 0xc) == 0) {
          write_multi_char(0x20,iVar8,param_1,&local_234);
        }
        write_string(&local_246,local_224,param_1,&local_234);
        if ((((uint)puVar12 & 8) != 0) && (((uint)puVar12 & 4) == 0)) {
          write_multi_char(0x30,(uint)local_230,param_1,&local_234);
        }
        if ((local_220 == 0) || ((int)puVar10 < 1)) {
          write_string((char *)local_240,(int)puVar10,param_1,&local_234);
        }
        else {
          local_23c = CONCAT44(local_23c._4_4_,puVar10 + -1);
          pwVar11 = local_240;
          do {
            wVar2 = *pwVar11;
            pwVar11 = pwVar11 + 1;
            iVar8 = _wctomb(local_244,wVar2);
            if (iVar8 < 1) break;
            write_string(local_244,iVar8,param_1,&local_234);
            iVar8 = (uint)local_23c;
            local_23c = CONCAT44(local_23c._4_4_,(uint)local_23c + -1);
          } while (iVar8 != 0);
        }
        if (((uint)puVar12 & 4) != 0) {
          write_multi_char(0x20,(uint)local_230,param_1,&local_234);
        }
      }
    }
    bVar9 = *param_2;
    param_2 = param_2 + 1;
  } while( true );
}



// Library Function - Single Match
//  _write_char
// 
// Library: Visual Studio 1998 Release

void __cdecl write_char(int param_1,FILE *param_2,int *param_3)

{
  int iVar1;
  uint uVar2;
  
  iVar1 = param_2->_cnt + -1;
  param_2->_cnt = iVar1;
  if (iVar1 < 0) {
    uVar2 = __flsbuf(param_1,param_2);
  }
  else {
    *param_2->_ptr = (char)param_1;
    uVar2 = (uint)(byte)*param_2->_ptr;
    param_2->_ptr = param_2->_ptr + 1;
  }
  if (uVar2 == 0xffffffff) {
    *param_3 = -1;
    return;
  }
  *param_3 = *param_3 + 1;
  return;
}



// Library Function - Single Match
//  _write_multi_char
// 
// Library: Visual Studio 1998 Release

void __cdecl write_multi_char(int param_1,int param_2,FILE *param_3,int *param_4)

{
  do {
    if (param_2 < 1) {
      return;
    }
    write_char(param_1,param_3,param_4);
    param_2 = param_2 + -1;
  } while (*param_4 != -1);
  return;
}



// Library Function - Single Match
//  _write_string
// 
// Library: Visual Studio 1998 Release

void __cdecl write_string(char *param_1,int param_2,FILE *param_3,int *param_4)

{
  do {
    if (param_2 < 1) {
      return;
    }
    write_char((int)*param_1,param_3,param_4);
    param_1 = param_1 + 1;
    param_2 = param_2 + -1;
  } while (*param_4 != -1);
  return;
}



// Library Function - Single Match
//  _get_int_arg
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl get_int_arg(int *param_1)

{
  undefined4 *puVar1;
  
  puVar1 = (undefined4 *)*param_1;
  *param_1 = (int)(puVar1 + 1);
  return *puVar1;
}



// Library Function - Single Match
//  _get_int64_arg
// 
// Library: Visual Studio 1998 Release

undefined8 __cdecl get_int64_arg(int *param_1)

{
  undefined8 *puVar1;
  
  puVar1 = (undefined8 *)*param_1;
  *param_1 = (int)(puVar1 + 1);
  return *puVar1;
}



// Library Function - Single Match
//  _get_short_arg
// 
// Library: Visual Studio 1998 Release

undefined2 __cdecl get_short_arg(undefined4 *param_1)

{
  undefined2 *puVar1;
  
  puVar1 = (undefined2 *)*param_1;
  *param_1 = puVar1 + 2;
  return *puVar1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined5 __fastcall FUN_004153de(undefined4 param_1,undefined1 param_2)

{
  undefined4 in_EAX;
  undefined1 extraout_DL;
  int unaff_EBP;
  float10 in_ST0;
  float10 fVar1;
  float10 fVar2;
  undefined4 unaff_retaddr;
  
  fVar1 = ABS(in_ST0);
  *(ushort *)(unaff_EBP + -0xa0) =
       (ushort)(_DAT_0043ba9e < fVar1) << 8 | (ushort)(NAN(_DAT_0043ba9e) || NAN(fVar1)) << 10 |
       (ushort)(_DAT_0043ba9e == fVar1) << 0xe;
  if ((*(byte *)(unaff_EBP + -0x9f) & 0x41) == 0) {
    fVar2 = ROUND(in_ST0);
    fVar1 = (float10)0;
    *(ushort *)(unaff_EBP + -0xa0) =
         (ushort)(fVar2 < fVar1) << 8 | (ushort)(NAN(fVar2) || NAN(fVar1)) << 10 |
         (ushort)(fVar2 == fVar1) << 0xe;
    fVar2 = in_ST0 - fVar2;
    fVar1 = (float10)0;
    *(ushort *)(unaff_EBP + -0xa0) =
         (ushort)(fVar2 < fVar1) << 8 | (ushort)(NAN(fVar2) || NAN(fVar1)) << 10 |
         (ushort)(fVar2 == fVar1) << 0xe;
    f2xm1(ABS(fVar2));
    return CONCAT14(*(undefined1 *)(unaff_EBP + -0x9f),in_EAX);
  }
  fVar1 = (float10)0;
  *(ushort *)(unaff_EBP + -0xa0) =
       (ushort)(in_ST0 < fVar1) << 8 | (ushort)(NAN(in_ST0) || NAN(fVar1)) << 10 |
       (ushort)(in_ST0 == fVar1) << 0xe;
  if ((*(byte *)(unaff_EBP + -0x9f) & 1) == 0) {
    return CONCAT14(param_2,unaff_retaddr);
  }
  *(undefined1 *)(unaff_EBP + -0x90) = 4;
  FUN_00417186();
  return CONCAT14(extraout_DL,unaff_retaddr);
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 FUN_00415421(void)

{
  undefined4 uVar1;
  float10 in_ST0;
  
  if (ROUND(in_ST0) == in_ST0) {
    if (ROUND(in_ST0 * (float10)_DAT_0043bab2) == in_ST0 * (float10)_DAT_0043bab2) {
      uVar1 = 2;
    }
    else {
      uVar1 = 1;
    }
  }
  else {
    uVar1 = 0;
  }
  return uVar1;
}



// Library Function - Single Match
//  __cintrindisp2
// 
// Libraries: Visual Studio 1998, Visual Studio 2003, Visual Studio 2019

void __fastcall __cintrindisp2(undefined4 param_1,int param_2)

{
  __trandisp2(param_1,param_2);
  DAT_00452878 = 1;
  FUN_00415545();
  return;
}



// Library Function - Single Match
//  __cintrindisp1
// 
// Libraries: Visual Studio 1998, Visual Studio 2003, Visual Studio 2019

void __fastcall __cintrindisp1(undefined4 param_1,int param_2)

{
  __trandisp1(param_1,param_2);
  DAT_00452878 = 1;
  FUN_00415545();
  return;
}



// Library Function - Single Match
//  __ctrandisp2
// 
// Libraries: Visual Studio 1998, Visual Studio 2003, Visual Studio 2019

void __cdecl __ctrandisp2(uint param_1,int param_2,uint param_3,int param_4)

{
  undefined4 extraout_ECX;
  int extraout_EDX;
  
  __fload(param_1,param_2);
  __fload(param_3,param_4);
  __trandisp2(extraout_ECX,extraout_EDX);
  FUN_0041553e();
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_0041553e(void)

{
  char cVar1;
  int unaff_EBP;
  ushort in_FPUStatusWord;
  float10 in_ST0;
  
  DAT_00452878 = '\0';
  if (DAT_0043aa44 != 0) {
    DAT_00452878 = 0;
    return;
  }
  _DAT_00452870 = (double)in_ST0;
  cVar1 = *(char *)(unaff_EBP + -0x90);
  if (cVar1 != '\0') {
    if ((cVar1 != -1) && (cVar1 != -2)) {
      if (cVar1 == '\0') {
        DAT_00452878 = 0;
        return;
      }
      *(int *)(unaff_EBP + -0x8e) = (int)cVar1;
      goto LAB_00415613;
    }
    if (((ulonglong)_DAT_00452870 & 0x7ff0000000000000) == 0) {
      *(undefined4 *)(unaff_EBP + -0x8e) = 4;
      in_ST0 = (float10)fscale(in_ST0,(float10)1536.0);
      if (ABS(in_ST0) < (float10)2.2250738585072014e-308) {
        in_ST0 = in_ST0 * (float10)0.0;
      }
      goto LAB_00415613;
    }
    if ((DAT_00452876 & 0x7ff0) == 0x7ff0) {
      *(undefined4 *)(unaff_EBP + -0x8e) = 3;
      in_ST0 = (float10)fscale(in_ST0,(float10)-1536.0);
      if ((float10)1.79769313486232e+308 < ABS(in_ST0)) {
        in_ST0 = in_ST0 * (float10)INFINITY;
      }
      goto LAB_00415613;
    }
  }
  if ((*(ushort *)(unaff_EBP + -0xa4) & 0x20) != 0) {
    DAT_00452878 = 0;
    return;
  }
  if ((in_FPUStatusWord & 0x20) == 0) {
    DAT_00452878 = 0;
    return;
  }
  *(undefined4 *)(unaff_EBP + -0x8e) = 8;
LAB_00415613:
  *(int *)(unaff_EBP + -0x8a) = *(int *)(unaff_EBP + -0x94) + 1;
  if (DAT_00452878 == '\0') {
    *(undefined4 *)(unaff_EBP + -0x86) = *(undefined4 *)(unaff_EBP + 8);
    *(undefined4 *)(unaff_EBP + -0x82) = *(undefined4 *)(unaff_EBP + 0xc);
    if (*(char *)(*(int *)(unaff_EBP + -0x94) + 0xd) != '\x01') {
      *(undefined4 *)(unaff_EBP + -0x7e) = *(undefined4 *)(unaff_EBP + 0x10);
      *(undefined4 *)(unaff_EBP + -0x7a) = *(undefined4 *)(unaff_EBP + 0x14);
    }
  }
  *(double *)(unaff_EBP + -0x76) = (double)in_ST0;
  __87except((int)*(char *)(*(int *)(unaff_EBP + -0x94) + 0xe),(int *)(unaff_EBP + -0x8e),
             (ushort *)(unaff_EBP + -0xa4));
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00415545(void)

{
  char cVar1;
  int unaff_EBP;
  ushort in_FPUStatusWord;
  float10 in_ST0;
  
  if (DAT_0043aa44 != 0) {
    return;
  }
  _DAT_00452870 = (double)in_ST0;
  cVar1 = *(char *)(unaff_EBP + -0x90);
  if (cVar1 != '\0') {
    if ((cVar1 != -1) && (cVar1 != -2)) {
      if (cVar1 == '\0') {
        return;
      }
      *(int *)(unaff_EBP + -0x8e) = (int)cVar1;
      goto LAB_00415613;
    }
    if (((ulonglong)_DAT_00452870 & 0x7ff0000000000000) == 0) {
      *(undefined4 *)(unaff_EBP + -0x8e) = 4;
      in_ST0 = (float10)fscale(in_ST0,(float10)1536.0);
      if (ABS(in_ST0) < (float10)2.2250738585072014e-308) {
        in_ST0 = in_ST0 * (float10)0.0;
      }
      goto LAB_00415613;
    }
    if ((DAT_00452876 & 0x7ff0) == 0x7ff0) {
      *(undefined4 *)(unaff_EBP + -0x8e) = 3;
      in_ST0 = (float10)fscale(in_ST0,(float10)-1536.0);
      if ((float10)1.79769313486232e+308 < ABS(in_ST0)) {
        in_ST0 = in_ST0 * (float10)INFINITY;
      }
      goto LAB_00415613;
    }
  }
  if ((*(ushort *)(unaff_EBP + -0xa4) & 0x20) != 0) {
    return;
  }
  if ((in_FPUStatusWord & 0x20) == 0) {
    return;
  }
  *(undefined4 *)(unaff_EBP + -0x8e) = 8;
LAB_00415613:
  *(int *)(unaff_EBP + -0x8a) = *(int *)(unaff_EBP + -0x94) + 1;
  if (DAT_00452878 == '\0') {
    *(undefined4 *)(unaff_EBP + -0x86) = *(undefined4 *)(unaff_EBP + 8);
    *(undefined4 *)(unaff_EBP + -0x82) = *(undefined4 *)(unaff_EBP + 0xc);
    if (*(char *)(*(int *)(unaff_EBP + -0x94) + 0xd) != '\x01') {
      *(undefined4 *)(unaff_EBP + -0x7e) = *(undefined4 *)(unaff_EBP + 0x10);
      *(undefined4 *)(unaff_EBP + -0x7a) = *(undefined4 *)(unaff_EBP + 0x14);
    }
  }
  *(double *)(unaff_EBP + -0x76) = (double)in_ST0;
  __87except((int)*(char *)(*(int *)(unaff_EBP + -0x94) + 0xe),(int *)(unaff_EBP + -0x8e),
             (ushort *)(unaff_EBP + -0xa4));
  return;
}



// Library Function - Single Match
//  __ctrandisp1
// 
// Libraries: Visual Studio 1998, Visual Studio 2003, Visual Studio 2019

void __cdecl __ctrandisp1(uint param_1,int param_2)

{
  undefined4 extraout_ECX;
  int extraout_EDX;
  
  __fload(param_1,param_2);
  __trandisp1(extraout_ECX,extraout_EDX);
  FUN_0041553e();
  return;
}



// Library Function - Single Match
//  __fload
// 
// Libraries: Visual Studio 1998, Visual Studio 2003, Visual Studio 2019

float10 __cdecl __fload(uint param_1,int param_2)

{
  float10 fVar1;
  
  if ((param_2._2_2_ & 0x7ff0) == 0x7ff0) {
    fVar1 = (float10)CONCAT28(param_2._2_2_ | 0x7fff,
                              CONCAT44(param_2 << 0xb | param_1 >> 0x15,param_1));
  }
  else {
    fVar1 = (float10)(double)CONCAT26(param_2._2_2_,CONCAT24((undefined2)param_2,param_1));
  }
  return fVar1;
}



// Library Function - Single Match
//  __control87
// 
// Library: Visual Studio 1998 Release

uint __cdecl __control87(uint _NewValue,uint _Mask)

{
  uint uVar1;
  ushort in_FPUControlWord;
  
  uVar1 = __abstract_cw(in_FPUControlWord);
  __hw_cw();
  return ~_Mask & uVar1 | _NewValue & _Mask;
}



// Library Function - Single Match
//  __controlfp
// 
// Library: Visual Studio 1998 Release

uint __cdecl __controlfp(uint _NewValue,uint _Mask)

{
  uint uVar1;
  
  uVar1 = __control87(_NewValue,_Mask & 0xfff7ffff);
  return uVar1;
}



// Library Function - Single Match
//  __abstract_cw
// 
// Library: Visual Studio 1998 Release

uint __cdecl __abstract_cw(ushort param_1)

{
  uint uVar1;
  ushort uVar2;
  
  uVar1 = 0;
  if ((param_1 & 1) != 0) {
    uVar1 = 0x10;
  }
  if ((param_1 & 4) != 0) {
    uVar1 = uVar1 | 8;
  }
  if ((param_1 & 8) != 0) {
    uVar1 = uVar1 | 4;
  }
  if ((param_1 & 0x10) != 0) {
    uVar1 = uVar1 | 2;
  }
  if ((param_1 & 0x20) != 0) {
    uVar1 = uVar1 | 1;
  }
  if ((param_1 & 2) != 0) {
    uVar1 = uVar1 | 0x80000;
  }
  uVar2 = param_1 & 0xc00;
  if (uVar2 == 0x400) {
    uVar1 = uVar1 | 0x100;
  }
  else if (uVar2 == 0x800) {
    uVar1 = uVar1 | 0x200;
  }
  else if (uVar2 == 0xc00) {
    uVar1 = uVar1 | 0x300;
  }
  if ((param_1 & 0x300) == 0) {
    uVar1 = uVar1 | 0x20000;
  }
  else if ((param_1 & 0x300) == 0x200) {
    uVar1 = uVar1 | 0x10000;
  }
  if ((param_1 & 0x1000) != 0) {
    uVar1 = uVar1 | 0x40000;
  }
  return uVar1;
}



// Library Function - Single Match
//  __hw_cw
// 
// Library: Visual Studio 1998 Release

void __hw_cw(void)

{
  return;
}



void FUN_00415890(void)

{
  __amsg_exit(2);
  return;
}



// Library Function - Single Match
//  __isctype
// 
// Library: Visual Studio 1998 Release

int __cdecl __isctype(int _C,int _Type)

{
  LPCSTR _LpSrcStr;
  BOOL BVar1;
  byte bVar2;
  BOOL unaff_EBX;
  undefined4 local_6;
  undefined1 local_2;
  
  if (_C + 1U < 0x101) {
    return (uint)*(ushort *)(PTR_DAT_0043bb90 + _C * 2) & _Type;
  }
  bVar2 = (byte)((uint)_C >> 8);
  if ((PTR_DAT_0043bb90[(uint)bVar2 * 2 + 1] & 0x80) == 0) {
    _LpSrcStr = (LPCSTR)0x1;
    local_6._0_3_ = CONCAT12((char)_C,(undefined2)local_6);
    local_6 = (uint)(uint3)local_6;
  }
  else {
    _LpSrcStr = (LPCSTR)0x2;
    local_6._0_3_ = CONCAT12(bVar2,(undefined2)local_6);
    local_2 = 0;
    local_6 = CONCAT13((char)_C,(uint3)local_6);
  }
  BVar1 = ___crtGetStringTypeA
                    ((_locale_t)0x1,(int)&local_6 + 2,_LpSrcStr,(int)&local_6,(LPWORD)0x0,0,
                     unaff_EBX);
  if (BVar1 == 0) {
    return 0;
  }
  return local_6 & 0xffff & _Type;
}



// Library Function - Single Match
//  _tolower
// 
// Library: Visual Studio 1998 Release

int __cdecl _tolower(int _C)

{
  uint uVar1;
  LPCSTR _LpSrcStr;
  int iVar2;
  int unaff_EBX;
  uint in_stack_fffffff8;
  byte local_4;
  byte local_3;
  undefined1 local_2;
  
  if (DAT_0043bdd8 == (_locale_t)0x0) {
    if ((0x40 < _C) && (_C < 0x5b)) {
      _C = _C + 0x20;
    }
    return _C;
  }
  if (_C < 0x100) {
    if (DAT_0043bb80 < 2) {
      uVar1 = *(ushort *)(PTR_DAT_0043bb90 + _C * 2) & 1;
    }
    else {
      uVar1 = __isctype(_C,1);
    }
    if (uVar1 == 0) {
      return _C;
    }
  }
  local_4 = (byte)((uint)_C >> 8);
  if ((PTR_DAT_0043bb90[(uint)local_4 * 2 + 1] & 0x80) == 0) {
    _LpSrcStr = (LPCSTR)0x1;
    local_3 = 0;
    local_4 = (byte)_C;
  }
  else {
    _LpSrcStr = (LPCSTR)0x2;
    local_2 = 0;
    local_3 = (byte)_C;
  }
  iVar2 = ___crtLCMapStringA(DAT_0043bdd8,(LPCWSTR)0x100,(DWORD)&local_4,_LpSrcStr,
                             (int)&stack0xfffffff8,(LPSTR)0x3,0,unaff_EBX,in_stack_fffffff8);
  if (iVar2 == 0) {
    return _C;
  }
  if (iVar2 == 1) {
    return in_stack_fffffff8 & 0xff;
  }
  return in_stack_fffffff8 & 0xffff;
}



// Library Function - Single Match
//  __ZeroTail
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl __ZeroTail(int param_1,int param_2)

{
  byte bVar1;
  int iVar2;
  int *piVar3;
  
  iVar2 = (int)(param_2 + (param_2 >> 0x1f & 0x1fU)) >> 5;
  bVar1 = (byte)(param_2 >> 0x1f);
  if ((*(uint *)(param_1 + iVar2 * 4) &
      ~(-1 << (0x1f - ((((byte)param_2 ^ bVar1) - bVar1 & 0x1f ^ bVar1) - bVar1) & 0x1f))) != 0) {
    return 0;
  }
  iVar2 = iVar2 + 1;
  if (iVar2 < 3) {
    piVar3 = (int *)(param_1 + iVar2 * 4);
    do {
      if (*piVar3 != 0) {
        return 0;
      }
      piVar3 = piVar3 + 1;
      iVar2 = iVar2 + 1;
    } while (iVar2 < 3);
  }
  return 1;
}



// Library Function - Single Match
//  __IncMan
// 
// Library: Visual Studio 1998 Release

void __cdecl __IncMan(int param_1,int param_2)

{
  byte bVar1;
  int iVar2;
  int iVar3;
  uint *puVar4;
  
  iVar2 = (int)(param_2 + (param_2 >> 0x1f & 0x1fU)) >> 5;
  bVar1 = (byte)(param_2 >> 0x1f);
  puVar4 = (uint *)(param_1 + iVar2 * 4);
  iVar3 = ___addl(*puVar4,1 << (0x1f - ((((byte)param_2 ^ bVar1) - bVar1 & 0x1f ^ bVar1) - bVar1) &
                               0x1f),puVar4);
  iVar2 = iVar2 + -1;
  if (-1 < iVar2) {
    puVar4 = (uint *)(param_1 + iVar2 * 4);
    do {
      if (iVar3 == 0) {
        return;
      }
      iVar3 = ___addl(*puVar4,1,puVar4);
      iVar2 = iVar2 + -1;
      puVar4 = puVar4 + -1;
    } while (-1 < iVar2);
  }
  return;
}



// Library Function - Single Match
//  __RoundMan
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl __RoundMan(int param_1,int param_2)

{
  uint *puVar1;
  byte bVar2;
  int iVar3;
  int iVar4;
  undefined4 *puVar5;
  undefined4 local_4;
  
  local_4 = 0;
  iVar3 = (int)(param_2 + (param_2 >> 0x1f & 0x1fU)) >> 5;
  bVar2 = (byte)(param_2 >> 0x1f);
  bVar2 = 0x1f - ((((byte)param_2 ^ bVar2) - bVar2 & 0x1f ^ bVar2) - bVar2);
  puVar1 = (uint *)(param_1 + iVar3 * 4);
  if ((*puVar1 & 1 << (bVar2 & 0x1f)) != 0) {
    iVar4 = __ZeroTail(param_1,param_2 + 1);
    if (iVar4 == 0) {
      local_4 = __IncMan(param_1,param_2 + -1);
    }
  }
  iVar3 = iVar3 + 1;
  *puVar1 = *puVar1 & -1 << (bVar2 & 0x1f);
  if (iVar3 < 3) {
    puVar5 = (undefined4 *)(param_1 + iVar3 * 4);
    for (iVar4 = 3 - iVar3; iVar4 != 0; iVar4 = iVar4 + -1) {
      *puVar5 = 0;
      puVar5 = puVar5 + 1;
    }
  }
  return local_4;
}



// Library Function - Single Match
//  __CopyMan
// 
// Library: Visual Studio 1998 Release

void __cdecl __CopyMan(undefined4 *param_1,undefined4 *param_2)

{
  undefined4 uVar1;
  int iVar2;
  
  iVar2 = 3;
  do {
    uVar1 = *param_2;
    param_2 = param_2 + 1;
    *param_1 = uVar1;
    param_1 = param_1 + 1;
    iVar2 = iVar2 + -1;
  } while (iVar2 != 0);
  return;
}



// Library Function - Single Match
//  __FillZeroMan
// 
// Library: Visual Studio 1998 Release

void __cdecl __FillZeroMan(undefined4 *param_1)

{
  *param_1 = 0;
  param_1[1] = 0;
  param_1[2] = 0;
  return;
}



// Library Function - Single Match
//  __IsZeroMan
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl __IsZeroMan(int *param_1)

{
  int iVar1;
  
  iVar1 = 0;
  do {
    if (*param_1 != 0) {
      return 0;
    }
    param_1 = param_1 + 1;
    iVar1 = iVar1 + 1;
  } while (iVar1 < 3);
  return 1;
}



// Library Function - Single Match
//  __ShrMan
// 
// Library: Visual Studio 1998 Release

void __cdecl __ShrMan(uint *param_1,int param_2)

{
  uint uVar1;
  byte bVar2;
  int iVar3;
  uint uVar4;
  uint *puVar5;
  int iVar6;
  uint *puVar7;
  uint uVar8;
  
  iVar3 = (int)(param_2 + (param_2 >> 0x1f & 0x1fU)) >> 5;
  bVar2 = (byte)(param_2 >> 0x1f);
  bVar2 = (((byte)param_2 ^ bVar2) - bVar2 & 0x1f ^ bVar2) - bVar2;
  iVar6 = 3;
  uVar4 = 0;
  puVar5 = param_1;
  do {
    uVar1 = *puVar5;
    uVar8 = uVar1 >> (bVar2 & 0x1f);
    *puVar5 = uVar8;
    uVar8 = uVar8 | uVar4;
    uVar4 = (uVar1 & ~(-1 << (bVar2 & 0x1f))) << (0x20 - bVar2 & 0x1f);
    iVar6 = iVar6 + -1;
    *puVar5 = uVar8;
    puVar5 = puVar5 + 1;
  } while (iVar6 != 0);
  iVar6 = 2;
  puVar7 = param_1 + 2;
  puVar5 = param_1 + (2 - iVar3);
  do {
    if (iVar6 < iVar3) {
      *puVar7 = 0;
    }
    else {
      *puVar7 = *puVar5;
    }
    puVar5 = puVar5 + -1;
    puVar7 = puVar7 + -1;
    iVar6 = iVar6 + -1;
  } while (-1 < iVar6);
  return;
}



// Library Function - Single Match
//  __ld12cvt
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl __ld12cvt(ushort *param_1,uint *param_2,int *param_3)

{
  ushort uVar1;
  int iVar2;
  undefined4 uVar3;
  uint uVar4;
  int iVar5;
  uint local_18;
  uint local_14;
  int local_10;
  undefined4 local_c [3];
  
  uVar1 = param_1[5];
  uVar4 = uVar1 & 0x7fff;
  iVar5 = uVar4 - 0x3fff;
  local_14 = *(uint *)(param_1 + 1);
  local_18 = *(uint *)(param_1 + 3);
  local_10 = (uint)*param_1 << 0x10;
  if (iVar5 == -0x3fff) {
    iVar5 = 0;
    iVar2 = __IsZeroMan((int *)&local_18);
    if (iVar2 == 0) {
      __FillZeroMan(&local_18);
      uVar3 = 2;
    }
    else {
      uVar3 = 0;
    }
  }
  else {
    __CopyMan(local_c,&local_18);
    iVar2 = __RoundMan((int)&local_18,param_3[2]);
    if (iVar2 != 0) {
      iVar5 = uVar4 - 0x3ffe;
    }
    iVar2 = param_3[1];
    if (iVar5 < iVar2 - param_3[2]) {
      iVar5 = 0;
      __FillZeroMan(&local_18);
      uVar3 = 2;
    }
    else if (iVar2 < iVar5) {
      if (iVar5 < *param_3) {
        iVar5 = iVar5 + param_3[5];
        local_18 = local_18 & 0x7fffffff;
        __ShrMan(&local_18,param_3[3]);
        uVar3 = 0;
      }
      else {
        __FillZeroMan(&local_18);
        local_18 = local_18 | 0x80000000;
        __ShrMan(&local_18,param_3[3]);
        iVar5 = param_3[5] + *param_3;
        uVar3 = 1;
      }
    }
    else {
      __CopyMan(&local_18,local_c);
      __ShrMan(&local_18,iVar2 - iVar5);
      __RoundMan((int)&local_18,param_3[2]);
      iVar5 = 0;
      __ShrMan(&local_18,param_3[3] + 1);
      uVar3 = 2;
    }
  }
  local_18 = iVar5 << (0x1fU - (char)param_3[3] & 0x1f) | ((uVar1 & 0x8000) == 0) - 1 & 0x80000000 |
             local_18;
  if (param_3[4] == 0x40) {
    param_2[1] = local_18;
    *param_2 = local_14;
    return uVar3;
  }
  if (param_3[4] == 0x20) {
    *param_2 = local_18;
  }
  return uVar3;
}



// Library Function - Multiple Matches With Different Base Names
//  __ld12tod
//  __ld12tof
// 
// Library: Visual Studio 1998 Release

INTRNCVT_STATUS __cdecl FID_conflict___ld12tod(_LDBL12 *_Ifp,_CRT_DOUBLE *_D)

{
  INTRNCVT_STATUS IVar1;
  
  IVar1 = __ld12cvt((ushort *)_Ifp,(uint *)_D,(int *)&DAT_0043bda0);
  return IVar1;
}



// Library Function - Multiple Matches With Different Base Names
//  __ld12tod
//  __ld12tof
// 
// Library: Visual Studio 1998 Release

INTRNCVT_STATUS __cdecl FID_conflict___ld12tod(_LDBL12 *_Ifp,_CRT_DOUBLE *_D)

{
  INTRNCVT_STATUS IVar1;
  
  IVar1 = __ld12cvt((ushort *)_Ifp,(uint *)_D,(int *)&DAT_0043bdb8);
  return IVar1;
}



// Library Function - Multiple Matches With Different Base Names
//  __atodbl
//  __atoflt
// 
// Library: Visual Studio 1998 Release

int __cdecl FID_conflict___atodbl(_CRT_FLOAT *_Result,char *_Str)

{
  INTRNCVT_STATUS IVar1;
  char *local_10;
  _LDBL12 local_c;
  
  ___strgtold12(&local_c,&local_10,_Str,0,0,0,0);
  IVar1 = FID_conflict___ld12tod(&local_c,(_CRT_DOUBLE *)_Result);
  return IVar1;
}



// Library Function - Multiple Matches With Different Base Names
//  __atodbl
//  __atoflt
// 
// Library: Visual Studio 1998 Release

int __cdecl FID_conflict___atodbl(_CRT_FLOAT *_Result,char *_Str)

{
  INTRNCVT_STATUS IVar1;
  char *local_10;
  _LDBL12 local_c;
  
  ___strgtold12(&local_c,&local_10,_Str,0,0,0,0);
  IVar1 = FID_conflict___ld12tod(&local_c,(_CRT_DOUBLE *)_Result);
  return IVar1;
}



// Library Function - Single Match
//  __fptostr
// 
// Library: Visual Studio 1998 Release

errno_t __cdecl __fptostr(char *_Buf,size_t _SizeInBytes,int _Digits,STRFLT _PtFlt)

{
  char cVar1;
  uint uVar2;
  uint uVar3;
  char *pcVar4;
  char *pcVar5;
  char *pcVar6;
  
  pcVar6 = _Buf + 1;
  pcVar4 = *(char **)(_Digits + 0xc);
  *_Buf = '0';
  pcVar5 = pcVar6;
  if (0 < (int)_SizeInBytes) {
    do {
      cVar1 = *pcVar4;
      if (cVar1 == '\0') {
        *pcVar5 = '0';
      }
      else {
        pcVar4 = pcVar4 + 1;
        *pcVar5 = cVar1;
      }
      pcVar5 = pcVar5 + 1;
      _SizeInBytes = _SizeInBytes - 1;
    } while (_SizeInBytes != 0);
  }
  *pcVar5 = '\0';
  if ((-1 < (int)_SizeInBytes) && ('4' < *pcVar4)) {
    pcVar5 = pcVar5 + -1;
    cVar1 = *pcVar5;
    while (cVar1 == '9') {
      *pcVar5 = '0';
      pcVar5 = pcVar5 + -1;
      cVar1 = *pcVar5;
    }
    *pcVar5 = *pcVar5 + '\x01';
  }
  if (*_Buf == '1') {
    *(int *)(_Digits + 4) = *(int *)(_Digits + 4) + 1;
    return _SizeInBytes;
  }
  uVar2 = 0xffffffff;
  do {
    pcVar4 = pcVar6;
    if (uVar2 == 0) break;
    uVar2 = uVar2 - 1;
    pcVar4 = pcVar6 + 1;
    cVar1 = *pcVar6;
    pcVar6 = pcVar4;
  } while (cVar1 != '\0');
  uVar2 = ~uVar2;
  pcVar6 = pcVar4 + -uVar2;
  for (uVar3 = uVar2 >> 2; uVar3 != 0; uVar3 = uVar3 - 1) {
    *(undefined4 *)_Buf = *(undefined4 *)pcVar6;
    pcVar6 = pcVar6 + 4;
    _Buf = _Buf + 4;
  }
  for (uVar3 = uVar2 & 3; uVar3 != 0; uVar3 = uVar3 - 1) {
    *_Buf = *pcVar6;
    pcVar6 = pcVar6 + 1;
    _Buf = _Buf + 1;
  }
  return uVar2;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __fltout
// 
// Library: Visual Studio 1998 Release

undefined * __fltout(void)

{
  uint local_c;
  uint local_8;
  ushort local_4;
  
  ___dtold(&local_c,(uint *)&stack0x00000004);
  _DAT_004528a8 = _I10_OUTPUT(local_c,local_8,local_4,0x11,0,&DAT_00452880);
  _DAT_004528ac = &DAT_00452884;
  _DAT_004528a0 = (int)DAT_00452882;
  _DAT_004528a4 = (int)DAT_00452880;
  return &DAT_004528a0;
}



// Library Function - Single Match
//  ___dtold
// 
// Library: Visual Studio 1998 Release

void __cdecl ___dtold(uint *param_1,uint *param_2)

{
  ushort uVar1;
  uint uVar2;
  uint uVar3;
  uint uVar4;
  ushort uVar5;
  
  uVar1 = *(ushort *)((int)param_2 + 6);
  uVar5 = (uVar1 & 0x7ff0) >> 4;
  uVar4 = 0x80000000;
  uVar2 = param_2[1];
  uVar3 = *param_2;
  if (uVar5 == 0) {
    if (((uVar2 & 0xfffff) == 0) && (uVar3 == 0)) {
      *(undefined2 *)(param_1 + 2) = 0;
      param_1[1] = 0;
      *param_1 = 0;
      return;
    }
    uVar5 = 0x3c01;
    uVar4 = 0;
  }
  else if (uVar5 == 0x7ff) {
    uVar5 = 0x7fff;
  }
  else {
    uVar5 = uVar5 + 0x3c00;
  }
  *param_1 = uVar3 << 0xb;
  param_1[1] = (uVar2 & 0xfffff) << 0xb | uVar3 >> 0x15 | uVar4;
  while (uVar4 == 0) {
    uVar5 = uVar5 - 1;
    uVar2 = *param_1;
    uVar3 = param_1[1];
    *param_1 = uVar2 * 2;
    param_1[1] = uVar2 >> 0x1f | uVar3 * 2;
    uVar4 = uVar3 * 2 & 0x80000000;
  }
  *(ushort *)(param_1 + 2) = uVar5 | uVar1 & 0x8000;
  return;
}



// Library Function - Multiple Matches With Different Base Names
//  _memcpy
//  _memmove
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release

void * __cdecl FID_conflict__memcpy(void *_Dst,void *_Src,size_t _Size)

{
  uint uVar1;
  int in_EDX;
  uint uVar2;
  undefined4 *puVar3;
  undefined1 *puVar4;
  undefined4 *puVar5;
  undefined1 *puVar6;
  
  if ((_Src < _Dst) && (_Dst < (void *)((int)_Src + _Size))) {
    puVar3 = (undefined4 *)((int)_Src + _Size);
    puVar5 = (undefined4 *)((int)_Dst + _Size);
    if (((uint)puVar5 & 3) == 0) {
      uVar1 = _Size >> 2;
      while( true ) {
        puVar5 = puVar5 + -1;
        puVar3 = puVar3 + -1;
        if (uVar1 == 0) break;
        uVar1 = uVar1 - 1;
        *puVar5 = *puVar3;
      }
      switch(_Size & 3) {
      case 1:
switchD_004161d9_caseD_1:
        *(undefined1 *)((int)puVar5 + 3) = *(undefined1 *)((int)puVar3 + 3);
        return _Dst;
      case 2:
switchD_004161d9_caseD_2:
        *(undefined2 *)((int)puVar5 + 2) = *(undefined2 *)((int)puVar3 + 2);
        return _Dst;
      case 3:
switchD_004161d9_caseD_3:
        *(undefined2 *)((int)puVar5 + 2) = *(undefined2 *)((int)puVar3 + 2);
        *(undefined1 *)((int)puVar5 + 1) = *(undefined1 *)((int)puVar3 + 1);
        return _Dst;
      }
    }
    else {
      puVar4 = (undefined1 *)((int)puVar3 + -1);
      puVar6 = (undefined1 *)((int)puVar5 + -1);
      if (_Size < 0xd) {
        for (; _Size != 0; _Size = _Size - 1) {
          *puVar6 = *puVar4;
          puVar4 = puVar4 + -1;
          puVar6 = puVar6 + -1;
        }
        return _Dst;
      }
      uVar2 = -in_EDX & 3;
      uVar1 = _Size - uVar2;
      for (; uVar2 != 0; uVar2 = uVar2 - 1) {
        *puVar6 = *puVar4;
        puVar4 = puVar4 + -1;
        puVar6 = puVar6 + -1;
      }
      puVar3 = (undefined4 *)(puVar4 + -3);
      puVar5 = (undefined4 *)(puVar6 + -3);
      for (uVar2 = uVar1 >> 2; uVar2 != 0; uVar2 = uVar2 - 1) {
        *puVar5 = *puVar3;
        puVar3 = puVar3 + -1;
        puVar5 = puVar5 + -1;
      }
      switch(uVar1 & 3) {
      case 1:
        goto switchD_004161d9_caseD_1;
      case 2:
        goto switchD_004161d9_caseD_2;
      case 3:
        goto switchD_004161d9_caseD_3;
      }
    }
    return _Dst;
  }
  puVar3 = (undefined4 *)_Dst;
  if (((uint)_Dst & 3) == 0) {
                    // WARNING: Load size is inaccurate
    for (uVar1 = _Size >> 2; uVar1 != 0; uVar1 = uVar1 - 1) {
      *puVar3 = *_Src;
      _Src = (undefined4 *)((int)_Src + 4);
      puVar3 = puVar3 + 1;
    }
    switch(_Size & 3) {
    case 1:
switchD_00416140_caseD_1:
                    // WARNING: Load size is inaccurate
      *(undefined1 *)puVar3 = *_Src;
      return _Dst;
    case 2:
switchD_00416140_caseD_2:
                    // WARNING: Load size is inaccurate
      *(undefined2 *)puVar3 = *_Src;
      return _Dst;
    case 3:
switchD_00416140_caseD_3:
                    // WARNING: Load size is inaccurate
      *(undefined2 *)puVar3 = *_Src;
      *(undefined1 *)((int)puVar3 + 2) = *(undefined1 *)((int)_Src + 2);
      return _Dst;
    }
  }
  else {
    puVar4 = (undefined1 *)_Dst;
    if (_Size < 0xd) {
                    // WARNING: Load size is inaccurate
      for (; _Size != 0; _Size = _Size - 1) {
        *puVar4 = *_Src;
        _Src = (undefined1 *)((int)_Src + 1);
        puVar4 = puVar4 + 1;
      }
      return _Dst;
    }
    uVar2 = -(int)_Dst & 3;
    uVar1 = _Size - uVar2;
                    // WARNING: Load size is inaccurate
    for (; uVar2 != 0; uVar2 = uVar2 - 1) {
      *(undefined1 *)puVar3 = *_Src;
      _Src = (undefined4 *)((int)_Src + 1);
      puVar3 = (undefined4 *)((int)puVar3 + 1);
    }
                    // WARNING: Load size is inaccurate
    for (uVar2 = uVar1 >> 2; uVar2 != 0; uVar2 = uVar2 - 1) {
      *puVar3 = *_Src;
      _Src = (undefined4 *)((int)_Src + 4);
      puVar3 = puVar3 + 1;
    }
    switch(uVar1 & 3) {
    case 1:
      goto switchD_00416140_caseD_1;
    case 2:
      goto switchD_00416140_caseD_2;
    case 3:
      goto switchD_00416140_caseD_3;
    }
  }
  return _Dst;
}



// Library Function - Single Match
//  ___crtMessageBoxA
// 
// Library: Visual Studio 1998 Release

int __cdecl ___crtMessageBoxA(LPCSTR _LpText,LPCSTR _LpCaption,UINT _UType)

{
  HMODULE hModule;
  int iVar1;
  
  iVar1 = 0;
  if (DAT_0043bdec != (FARPROC)0x0) {
LAB_004162af:
    if (DAT_0043bdf0 != (FARPROC)0x0) {
      iVar1 = (*DAT_0043bdf0)();
    }
    if ((iVar1 != 0) && (DAT_0043bdf4 != (FARPROC)0x0)) {
      iVar1 = (*DAT_0043bdf4)(iVar1);
    }
    iVar1 = (*DAT_0043bdec)(iVar1,_LpText,_LpCaption,_UType);
    return iVar1;
  }
  hModule = LoadLibraryA("user32.dll");
  if (hModule != (HMODULE)0x0) {
    DAT_0043bdec = GetProcAddress(hModule,"MessageBoxA");
    if (DAT_0043bdec != (FARPROC)0x0) {
      DAT_0043bdf0 = GetProcAddress(hModule,"GetActiveWindow");
      DAT_0043bdf4 = GetProcAddress(hModule,"GetLastActivePopup");
      goto LAB_004162af;
    }
  }
  return 0;
}



// Library Function - Single Match
//  _strncpy
// 
// Library: Visual Studio 1998 Release

char * __cdecl _strncpy(char *_Dest,char *_Source,size_t _Count)

{
  uint uVar1;
  uint uVar2;
  char cVar3;
  uint uVar4;
  uint *puVar5;
  
  if (_Count == 0) {
    return _Dest;
  }
  puVar5 = (uint *)_Dest;
  if (((uint)_Source & 3) != 0) {
    while( true ) {
      uVar4 = *(uint *)_Source;
      _Source = (char *)((int)_Source + 1);
      *(char *)puVar5 = (char)uVar4;
      puVar5 = (uint *)((int)puVar5 + 1);
      _Count = _Count - 1;
      if (_Count == 0) {
        return _Dest;
      }
      if ((char)uVar4 == '\0') break;
      if (((uint)_Source & 3) == 0) {
        uVar4 = _Count >> 2;
        goto joined_r0x0041633e;
      }
    }
    do {
      if (((uint)puVar5 & 3) == 0) {
        uVar4 = _Count >> 2;
        cVar3 = '\0';
        if (uVar4 == 0) goto LAB_0041637b;
        goto LAB_004163e9;
      }
      *(char *)puVar5 = '\0';
      puVar5 = (uint *)((int)puVar5 + 1);
      _Count = _Count - 1;
    } while (_Count != 0);
    return _Dest;
  }
  uVar4 = _Count >> 2;
  if (uVar4 != 0) {
    do {
      uVar1 = *(uint *)_Source;
      uVar2 = *(uint *)_Source;
      _Source = (char *)((int)_Source + 4);
      if (((uVar1 ^ 0xffffffff ^ uVar1 + 0x7efefeff) & 0x81010100) != 0) {
        if ((char)uVar2 == '\0') {
          *puVar5 = 0;
joined_r0x004163e5:
          while( true ) {
            uVar4 = uVar4 - 1;
            puVar5 = puVar5 + 1;
            if (uVar4 == 0) break;
LAB_004163e9:
            *puVar5 = 0;
          }
          cVar3 = '\0';
          _Count = _Count & 3;
          if (_Count != 0) goto LAB_0041637b;
          return _Dest;
        }
        if ((char)(uVar2 >> 8) == '\0') {
          *puVar5 = uVar2 & 0xff;
          goto joined_r0x004163e5;
        }
        if ((uVar2 & 0xff0000) == 0) {
          *puVar5 = uVar2 & 0xffff;
          goto joined_r0x004163e5;
        }
        if ((uVar2 & 0xff000000) == 0) {
          *puVar5 = uVar2;
          goto joined_r0x004163e5;
        }
      }
      *puVar5 = uVar2;
      puVar5 = puVar5 + 1;
      uVar4 = uVar4 - 1;
joined_r0x0041633e:
    } while (uVar4 != 0);
    _Count = _Count & 3;
    if (_Count == 0) {
      return _Dest;
    }
  }
  do {
    cVar3 = (char)*(uint *)_Source;
    _Source = (char *)((int)_Source + 1);
    *(char *)puVar5 = cVar3;
    puVar5 = (uint *)((int)puVar5 + 1);
    if (cVar3 == '\0') {
      while (_Count = _Count - 1, _Count != 0) {
LAB_0041637b:
        *(char *)puVar5 = cVar3;
        puVar5 = (uint *)((int)puVar5 + 1);
      }
      return _Dest;
    }
    _Count = _Count - 1;
  } while (_Count != 0);
  return _Dest;
}



// Library Function - Single Match
//  __alloc_osfhnd
// 
// Library: Visual Studio 1998 Release

int __cdecl __alloc_osfhnd(void)

{
  undefined4 *puVar1;
  undefined4 *puVar2;
  int *piVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  
  iVar5 = -1;
  iVar6 = 0;
  iVar4 = 0;
  piVar3 = &DAT_00454580;
  do {
    puVar1 = (undefined4 *)*piVar3;
    if (puVar1 == (undefined4 *)0x0) {
      puVar1 = (undefined4 *)_malloc(0x100);
      if (puVar1 != (undefined4 *)0x0) {
        DAT_00454680 = DAT_00454680 + 0x20;
        (&DAT_00454580)[iVar6] = puVar1;
        if (puVar1 < puVar1 + 0x40) {
          do {
            *(undefined1 *)(puVar1 + 1) = 0;
            puVar2 = puVar1 + 2;
            *puVar1 = 0xffffffff;
            *(undefined1 *)((int)puVar1 + 5) = 10;
            puVar1 = puVar2;
          } while (puVar2 < (undefined4 *)((&DAT_00454580)[iVar6] + 0x100));
        }
        iVar5 = iVar6 << 5;
      }
      return iVar5;
    }
    puVar2 = puVar1 + 0x40;
    for (; puVar1 < puVar2; puVar1 = puVar1 + 2) {
      if ((*(byte *)(puVar1 + 1) & 1) == 0) {
        *puVar1 = 0xffffffff;
        iVar5 = ((int)puVar1 - *piVar3 >> 3) + iVar4;
        break;
      }
    }
    if (iVar5 != -1) {
      return iVar5;
    }
    iVar4 = iVar4 + 0x20;
    piVar3 = piVar3 + 1;
    iVar6 = iVar6 + 1;
    if ((int *)0x45467f < piVar3) {
      return -1;
    }
  } while( true );
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __set_osfhnd
// 
// Library: Visual Studio 1998 Release

int __cdecl __set_osfhnd(int param_1,intptr_t param_2)

{
  int *piVar1;
  int iVar2;
  
  if ((uint)param_1 < DAT_00454680) {
    piVar1 = (int *)((int)&DAT_00454580 + ((int)(param_1 & 0xffffffe7U) >> 3));
    iVar2 = (param_1 & 0x1fU) * 8;
    if (*(int *)(*piVar1 + iVar2) == -1) {
      if (DAT_0043aab4 == 1) {
        if (param_1 == 0) {
          SetStdHandle(0xfffffff6,(HANDLE)param_2);
        }
        else if (param_1 == 1) {
          SetStdHandle(0xfffffff5,(HANDLE)param_2);
        }
        else if (param_1 == 2) {
          SetStdHandle(0xfffffff4,(HANDLE)param_2);
        }
      }
      *(intptr_t *)(*piVar1 + iVar2) = param_2;
      return 0;
    }
  }
  _DAT_0043aa58 = 9;
  DAT_0043aa5c = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __free_osfhnd
// 
// Library: Visual Studio 1998 Release

int __cdecl __free_osfhnd(int param_1)

{
  int *piVar1;
  int iVar2;
  int *piVar3;
  DWORD nStdHandle;
  
  if ((uint)param_1 < DAT_00454680) {
    piVar1 = (int *)((int)&DAT_00454580 + ((int)(param_1 & 0xffffffe7U) >> 3));
    iVar2 = (param_1 & 0x1fU) * 8;
    piVar3 = (int *)(*piVar1 + iVar2);
    if (((*(byte *)(piVar3 + 1) & 1) != 0) && (*piVar3 != -1)) {
      if (DAT_0043aab4 == 1) {
        if (param_1 == 0) {
          nStdHandle = 0xfffffff6;
        }
        else if (param_1 == 1) {
          nStdHandle = 0xfffffff5;
        }
        else {
          if (param_1 != 2) goto LAB_004165d6;
          nStdHandle = 0xfffffff4;
        }
        SetStdHandle(nStdHandle,(HANDLE)0x0);
      }
LAB_004165d6:
      *(undefined4 *)(*piVar1 + iVar2) = 0xffffffff;
      return 0;
    }
  }
  _DAT_0043aa58 = 9;
  DAT_0043aa5c = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __get_osfhandle
// 
// Library: Visual Studio 1998 Release

intptr_t __cdecl __get_osfhandle(int _FileHandle)

{
  intptr_t *piVar1;
  
  if (((uint)_FileHandle < DAT_00454680) &&
     (piVar1 = (intptr_t *)
               (*(int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3)) +
               (_FileHandle & 0x1fU) * 8), (*(byte *)(piVar1 + 1) & 1) != 0)) {
    return *piVar1;
  }
  _DAT_0043aa58 = 9;
  DAT_0043aa5c = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __commit
// 
// Library: Visual Studio 1998 Release

int __cdecl __commit(int _FileHandle)

{
  HANDLE hFile;
  BOOL BVar1;
  DWORD DVar2;
  
  if (((uint)_FileHandle < DAT_00454680) &&
     ((*(byte *)(*(int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3)) + 4 +
                (_FileHandle & 0x1fU) * 8) & 1) != 0)) {
    hFile = (HANDLE)__get_osfhandle(_FileHandle);
    BVar1 = FlushFileBuffers(hFile);
    DVar2 = 0;
    if (BVar1 == 0) {
      DVar2 = GetLastError();
    }
    if (DVar2 != 0) {
      _DAT_0043aa58 = 9;
      DAT_0043aa5c = DVar2;
      return -1;
    }
  }
  else {
    _DAT_0043aa58 = 9;
    DVar2 = 0xffffffff;
  }
  return DVar2;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __getbuf
// 
// Library: Visual Studio 1998 Release

void __cdecl __getbuf(FILE *_File)

{
  char *pcVar1;
  
  _DAT_0043ba68 = _DAT_0043ba68 + 1;
  pcVar1 = (char *)_malloc(0x1000);
  _File->_base = pcVar1;
  if (pcVar1 == (char *)0x0) {
    _File->_flag = _File->_flag | 4;
    _File->_base = (char *)&_File->_charbuf;
    _File->_bufsiz = 2;
  }
  else {
    _File->_flag = _File->_flag | 8;
    _File->_bufsiz = 0x1000;
  }
  _File->_ptr = _File->_base;
  _File->_cnt = 0;
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __lseeki64
// 
// Library: Visual Studio 1998 Release

longlong __cdecl __lseeki64(int _FileHandle,longlong _Offset,int _Origin)

{
  int *piVar1;
  int iVar2;
  byte *pbVar3;
  HANDLE hFile;
  DWORD DVar4;
  DWORD DVar5;
  LONG in_stack_00000008;
  LONG local_4;
  
  if ((uint)_FileHandle < DAT_00454680) {
    piVar1 = (int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3));
    iVar2 = (_FileHandle & 0x1fU) * 8;
    if ((*(byte *)(*piVar1 + 4 + iVar2) & 1) != 0) {
      local_4 = (LONG)_Offset;
      hFile = (HANDLE)__get_osfhandle(_FileHandle);
      if (hFile == (HANDLE)0xffffffff) {
        _DAT_0043aa58 = 9;
        return -1;
      }
      DVar4 = SetFilePointer(hFile,in_stack_00000008,&local_4,_Offset._4_4_);
      if (DVar4 == 0xffffffff) {
        DVar5 = GetLastError();
        if (DVar5 != 0) {
          __dosmaperr(DVar5);
          return -1;
        }
      }
      pbVar3 = (byte *)(*piVar1 + 4 + iVar2);
      *pbVar3 = *pbVar3 & 0xfd;
      return CONCAT44(local_4,DVar4);
    }
  }
  _DAT_0043aa58 = 9;
  DAT_0043aa5c = 0;
  return -1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __ftelli64
// 
// Library: Visual Studio 1998 Release

longlong __cdecl __ftelli64(FILE *_File)

{
  int *piVar1;
  int iVar2;
  uint _FileHandle;
  uint uVar3;
  byte bVar4;
  char *pcVar5;
  char *pcVar6;
  uint uVar7;
  uint uVar8;
  int unaff_EBP;
  longlong lVar9;
  undefined8 local_10;
  uint local_4;
  
  _FileHandle = _File->_file;
  if (_File->_cnt < 0) {
    _File->_cnt = 0;
  }
  local_10 = __lseeki64(_FileHandle,0x100000000,unaff_EBP);
  uVar7 = (uint)((ulonglong)local_10 >> 0x20);
  if ((uVar7 == 0 || local_10 < 0) && (local_10 < 0)) {
    return -1;
  }
  uVar3 = _File->_flag;
  if ((uVar3 & 0x108) == 0) {
    return local_10 - _File->_cnt;
  }
  pcVar5 = _File->_base;
  uVar8 = (int)_File->_ptr - (int)pcVar5;
  local_4 = uVar8;
  if ((uVar3 & 3) == 0) {
    if ((uVar3 & 0x80) == 0) {
      _DAT_0043aa58 = 0x16;
      return -1;
    }
  }
  else if ((*(byte *)(*(int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7) >> 3)) + 4 +
                     (_FileHandle & 0x1f) * 8) & 0x80) != 0) {
    for (; pcVar5 < _File->_ptr; pcVar5 = pcVar5 + 1) {
      if (*pcVar5 == '\n') {
        local_4 = local_4 + 1;
      }
    }
  }
  if (local_10 != 0) {
    if ((uVar3 & 1) != 0) {
      if (_File->_cnt == 0) {
        local_4 = 0;
      }
      else {
        uVar8 = _File->_cnt + uVar8;
        piVar1 = (int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7) >> 3));
        iVar2 = (_FileHandle & 0x1f) * 8;
        if ((*(byte *)(*piVar1 + 4 + iVar2) & 0x80) != 0) {
          lVar9 = __lseeki64(_FileHandle,0x200000000,unaff_EBP);
          if (lVar9 == local_10) {
            pcVar6 = _File->_base;
            pcVar5 = pcVar6 + uVar8;
            for (; pcVar6 < pcVar5; pcVar6 = pcVar6 + 1) {
              if (*pcVar6 == '\n') {
                uVar8 = uVar8 + 1;
              }
            }
            bVar4 = *(byte *)((int)&_File->_flag + 1) & 0x20;
          }
          else {
            __lseeki64(_FileHandle,(ulonglong)uVar7,unaff_EBP);
            if (((0x200 < uVar8) || ((_File->_flag & 8U) == 0)) ||
               (uVar8 = 0x200, (_File->_flag & 0x400U) != 0)) {
              uVar8 = _File->_bufsiz;
            }
            bVar4 = *(byte *)(*piVar1 + 4 + iVar2) & 4;
          }
          if (bVar4 != 0) {
            uVar8 = uVar8 + 1;
          }
        }
        local_10 = CONCAT44(uVar7 - ((uint)local_10 < uVar8),(uint)local_10 - uVar8);
      }
    }
    return CONCAT44(local_10._4_4_ + (uint)CARRY4(local_4,(uint)local_10),local_4 + (uint)local_10);
  }
  return (longlong)local_4;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __sopen
// 
// Library: Visual Studio 1998 Release

int __cdecl __sopen(char *_Filename,int _OpenFlag,int _ShareFlag,...)

{
  int *piVar1;
  byte *pbVar2;
  uint uVar3;
  HANDLE hFile;
  long lVar4;
  int iVar5;
  byte bVar6;
  DWORD DVar7;
  bool bVar8;
  uint in_stack_00000010;
  char cStack_19;
  DWORD local_18;
  uint local_14;
  DWORD local_10;
  _SECURITY_ATTRIBUTES local_c;
  
  local_c.lpSecurityDescriptor = (LPVOID)0x0;
  local_c.nLength = 0xc;
  bVar8 = (_OpenFlag & 0x80U) == 0;
  if (bVar8) {
    bVar6 = 0;
  }
  else {
    bVar6 = 0x10;
  }
  local_c.bInheritHandle = (BOOL)bVar8;
  if (((_OpenFlag & 0x8000U) == 0) && (((_OpenFlag & 0x4000U) != 0 || (DAT_0043bfb0 != 0x8000)))) {
    bVar6 = bVar6 | 0x80;
  }
  uVar3 = _OpenFlag & 3;
  if (uVar3 == 0) {
    local_14 = 0x80000000;
  }
  else if (uVar3 == 1) {
    local_14 = 0x40000000;
  }
  else {
    if (uVar3 != 2) {
      _DAT_0043aa58 = 0x16;
      DAT_0043aa5c = 0;
      return -1;
    }
    local_14 = 0xc0000000;
  }
  switch(_ShareFlag) {
  case 0x10:
    local_18 = 0;
    break;
  default:
    _DAT_0043aa58 = 0x16;
    DAT_0043aa5c = 0;
    return -1;
  case 0x20:
    local_18 = 1;
    break;
  case 0x30:
    local_18 = 2;
    break;
  case 0x40:
    local_18 = 3;
  }
  uVar3 = _OpenFlag & 0x700;
  if (uVar3 < 0x101) {
    if (uVar3 == 0x100) {
      local_10 = 4;
      goto LAB_00416b7b;
    }
    if (uVar3 != 0) {
      _DAT_0043aa58 = 0x16;
      DAT_0043aa5c = 0;
      return -1;
    }
LAB_00416b4b:
    local_10 = 3;
    goto LAB_00416b7b;
  }
  if (uVar3 < 0x301) {
    if (uVar3 == 0x300) {
      local_10 = 2;
      goto LAB_00416b7b;
    }
    if (uVar3 != 0x200) {
      _DAT_0043aa58 = 0x16;
      DAT_0043aa5c = 0;
      return -1;
    }
LAB_00416b5f:
    local_10 = 5;
  }
  else {
    if (uVar3 < 0x501) {
      if (uVar3 != 0x500) {
        if (uVar3 != 0x400) {
          _DAT_0043aa58 = 0x16;
          DAT_0043aa5c = 0;
          return -1;
        }
        goto LAB_00416b4b;
      }
    }
    else {
      if (uVar3 == 0x600) goto LAB_00416b5f;
      if (uVar3 != 0x700) {
        _DAT_0043aa58 = 0x16;
        DAT_0043aa5c = 0;
        return -1;
      }
    }
    local_10 = 1;
  }
LAB_00416b7b:
  DVar7 = 0x80;
  if (((_OpenFlag & 0x100U) != 0) && ((~DAT_0043aa60 & in_stack_00000010 & 0x80) == 0)) {
    DVar7 = 1;
  }
  if ((_OpenFlag & 0x40U) != 0) {
    local_14 = local_14 | 0x10000;
    DVar7 = DVar7 | 0x4000000;
  }
  if ((_OpenFlag & 0x1000U) != 0) {
    DVar7 = DVar7 | 0x100;
  }
  if ((_OpenFlag & 0x20U) == 0) {
    if ((_OpenFlag & 0x10U) != 0) {
      DVar7 = DVar7 | 0x10000000;
    }
  }
  else {
    DVar7 = DVar7 | 0x8000000;
  }
  uVar3 = __alloc_osfhnd();
  if (uVar3 == 0xffffffff) {
    _DAT_0043aa58 = 0x18;
    DAT_0043aa5c = 0;
    return -1;
  }
  hFile = CreateFileA(_Filename,local_14,local_18,&local_c,local_10,DVar7,(HANDLE)0x0);
  if (hFile == (HANDLE)0xffffffff) {
    DVar7 = GetLastError();
    __dosmaperr(DVar7);
    return -1;
  }
  DVar7 = GetFileType(hFile);
  if (DVar7 != 0) {
    if (DVar7 == 2) {
      bVar6 = bVar6 | 0x40;
    }
    else if (DVar7 == 3) {
      bVar6 = bVar6 | 8;
    }
    __set_osfhnd(uVar3,(intptr_t)hFile);
    piVar1 = (int *)((int)&DAT_00454580 + ((int)(uVar3 & 0xffffffe7) >> 3));
    local_14 = (uVar3 & 0x1f) * 8;
    *(byte *)(*piVar1 + 4 + local_14) = bVar6 | 1;
    local_18 = CONCAT31(local_18._1_3_,bVar6) & 0xffffff48;
    if ((((bVar6 & 0x48) == 0) && ((bVar6 & 0x80) != 0)) && ((_OpenFlag & 2U) != 0)) {
      lVar4 = __lseek(uVar3,-1,2);
      if (lVar4 == -1) {
        if (DAT_0043aa5c != 0x83) {
          __close(uVar3);
          return -1;
        }
      }
      else {
        cStack_19 = '\0';
        iVar5 = __read(uVar3,&cStack_19,1);
        if (((iVar5 == 0) && (cStack_19 == '\x1a')) && (iVar5 = __chsize(uVar3,lVar4), iVar5 == -1))
        {
          __close(uVar3);
          return -1;
        }
        lVar4 = __lseek(uVar3,0,0);
        if (lVar4 == -1) {
          __close(uVar3);
          return -1;
        }
      }
    }
    if (((char)local_18 == '\0') && ((_OpenFlag & 8U) != 0)) {
      pbVar2 = (byte *)(*piVar1 + 4 + local_14);
      *pbVar2 = *pbVar2 | 0x20;
    }
    return uVar3;
  }
  CloseHandle(hFile);
  DVar7 = GetLastError();
  __dosmaperr(DVar7);
  return -1;
}



// Library Function - Single Match
//  __isatty
// 
// Library: Visual Studio 1998 Release

int __cdecl __isatty(int _FileHandle)

{
  if (DAT_00454680 <= (uint)_FileHandle) {
    return 0;
  }
  return *(byte *)(*(int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3)) + 4 +
                  (_FileHandle & 0x1fU) * 8) & 0x40;
}



// Library Function - Single Match
//  __fcloseall
// 
// Library: Visual Studio 1998 Release

int __cdecl __fcloseall(void)

{
  FILE *_File;
  int iVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  
  iVar3 = 0;
  iVar4 = 3;
  if (3 < DAT_00454570) {
    iVar2 = 0xc;
    do {
      _File = *(FILE **)(DAT_00453564 + iVar2);
      if (_File != (FILE *)0x0) {
        if ((_File->_flag & 0x83U) != 0) {
          iVar1 = _fclose(_File);
          if (iVar1 != -1) {
            iVar3 = iVar3 + 1;
          }
        }
        if (0x4f < iVar2) {
          _free(*(void **)(DAT_00453564 + iVar2));
          *(undefined4 *)(DAT_00453564 + iVar2) = 0;
        }
      }
      iVar2 = iVar2 + 4;
      iVar4 = iVar4 + 1;
    } while (iVar4 < DAT_00454570);
  }
  return iVar3;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  _wctomb
// 
// Library: Visual Studio 1998 Release

int __cdecl _wctomb(char *_MbCh,wchar_t _WCh)

{
  int iVar1;
  BOOL local_4;
  
  if (_MbCh == (char *)0x0) {
    return 0;
  }
  if (DAT_0043bdd8 == 0) {
    if (0xff < (ushort)_WCh) {
      _DAT_0043aa58 = 0x2a;
      return -1;
    }
    *_MbCh = (char)_WCh;
    return 1;
  }
  local_4 = 0;
  iVar1 = WideCharToMultiByte(DAT_0043bde8,0x220,&_WCh,1,_MbCh,DAT_0043bb80,(LPCSTR)0x0,&local_4);
  if ((iVar1 == 0) || (local_4 != 0)) {
    _DAT_0043aa58 = 0x2a;
    iVar1 = -1;
  }
  return iVar1;
}



// Library Function - Single Match
//  __aulldiv
// 
// Library: Visual Studio 1998 Release

undefined8 __aulldiv(uint param_1,uint param_2,uint param_3,uint param_4)

{
  ulonglong uVar1;
  longlong lVar2;
  uint uVar3;
  int iVar4;
  uint uVar5;
  uint uVar6;
  uint uVar7;
  uint uVar8;
  uint uVar9;
  
  uVar3 = param_1;
  uVar8 = param_4;
  uVar6 = param_2;
  uVar9 = param_3;
  if (param_4 == 0) {
    uVar3 = param_2 / param_3;
    iVar4 = (int)(((ulonglong)param_2 % (ulonglong)param_3 << 0x20 | (ulonglong)param_1) /
                 (ulonglong)param_3);
  }
  else {
    do {
      uVar5 = uVar8 >> 1;
      uVar9 = uVar9 >> 1 | (uint)((uVar8 & 1) != 0) << 0x1f;
      uVar7 = uVar6 >> 1;
      uVar3 = uVar3 >> 1 | (uint)((uVar6 & 1) != 0) << 0x1f;
      uVar8 = uVar5;
      uVar6 = uVar7;
    } while (uVar5 != 0);
    uVar1 = CONCAT44(uVar7,uVar3) / (ulonglong)uVar9;
    iVar4 = (int)uVar1;
    lVar2 = (ulonglong)param_3 * (uVar1 & 0xffffffff);
    uVar3 = (uint)((ulonglong)lVar2 >> 0x20);
    uVar8 = uVar3 + iVar4 * param_4;
    if (((CARRY4(uVar3,iVar4 * param_4)) || (param_2 < uVar8)) ||
       ((param_2 <= uVar8 && (param_1 < (uint)lVar2)))) {
      iVar4 = iVar4 + -1;
    }
    uVar3 = 0;
  }
  return CONCAT44(uVar3,iVar4);
}



// Library Function - Single Match
//  __aullrem
// 
// Library: Visual Studio 1998 Release

undefined8 __aullrem(uint param_1,uint param_2,uint param_3,uint param_4)

{
  ulonglong uVar1;
  longlong lVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  int iVar6;
  int iVar7;
  uint uVar8;
  uint uVar9;
  uint uVar10;
  bool bVar11;
  
  uVar3 = param_1;
  uVar4 = param_4;
  uVar9 = param_2;
  uVar10 = param_3;
  if (param_4 == 0) {
    iVar6 = (int)(((ulonglong)param_2 % (ulonglong)param_3 << 0x20 | (ulonglong)param_1) %
                 (ulonglong)param_3);
    iVar7 = 0;
  }
  else {
    do {
      uVar5 = uVar4 >> 1;
      uVar10 = uVar10 >> 1 | (uint)((uVar4 & 1) != 0) << 0x1f;
      uVar8 = uVar9 >> 1;
      uVar3 = uVar3 >> 1 | (uint)((uVar9 & 1) != 0) << 0x1f;
      uVar4 = uVar5;
      uVar9 = uVar8;
    } while (uVar5 != 0);
    uVar1 = CONCAT44(uVar8,uVar3) / (ulonglong)uVar10;
    uVar3 = (int)uVar1 * param_4;
    lVar2 = (uVar1 & 0xffffffff) * (ulonglong)param_3;
    uVar9 = (uint)((ulonglong)lVar2 >> 0x20);
    uVar4 = (uint)lVar2;
    uVar10 = uVar9 + uVar3;
    if (((CARRY4(uVar9,uVar3)) || (param_2 < uVar10)) || ((param_2 <= uVar10 && (param_1 < uVar4))))
    {
      bVar11 = uVar4 < param_3;
      uVar4 = uVar4 - param_3;
      uVar10 = (uVar10 - param_4) - (uint)bVar11;
    }
    iVar6 = -(uVar4 - param_1);
    iVar7 = -(uint)(uVar4 - param_1 != 0) - ((uVar10 - param_2) - (uint)(uVar4 < param_1));
  }
  return CONCAT44(iVar7,iVar6);
}



// Library Function - Single Match
//  __trandisp1
// 
// Libraries: Visual Studio 1998, Visual Studio 2003, Visual Studio 2019

void __fastcall __trandisp1(undefined4 param_1,int param_2)

{
  float10 fVar1;
  byte bVar2;
  undefined2 uVar3;
  int unaff_EBP;
  float10 in_ST0;
  
  if (*(char *)(param_2 + 0xe) == '\x05') {
    uVar3 = (undefined2)
            CONCAT31((uint3)((byte)((ushort)*(undefined2 *)(unaff_EBP + -0xa4) >> 8) & 0xfe | 2),
                     0x3f);
  }
  else {
    uVar3 = 0x133f;
  }
  *(undefined2 *)(unaff_EBP + -0xa2) = uVar3;
  fVar1 = (float10)0;
  *(int *)(unaff_EBP + -0x94) = param_2;
  *(ushort *)(unaff_EBP + -0xa0) =
       (ushort)NAN(in_ST0) << 8 | (ushort)(in_ST0 < fVar1) << 9 | (ushort)(in_ST0 != fVar1) << 10 |
       (ushort)(in_ST0 == fVar1) << 0xe;
  *(undefined1 *)(unaff_EBP + -0x90) = 0;
  bVar2 = (char)(*(char *)(unaff_EBP + -0x9f) << 1) >> 1;
                    // WARNING: Could not recover jumptable at 0x004170e5. Too many branches
                    // WARNING: Treating indirect jump as call
  (**(code **)(param_2 + (char)(&DAT_0043be2d)[(byte)((bVar2 & 7) << 1 | (char)bVar2 < '\0')] + 0x10
              ))();
  return;
}



// Library Function - Single Match
//  __trandisp2
// 
// Libraries: Visual Studio 1998, Visual Studio 2003, Visual Studio 2019

void __fastcall __trandisp2(undefined4 param_1,int param_2)

{
  float10 fVar1;
  char cVar2;
  byte bVar3;
  undefined2 uVar4;
  int unaff_EBP;
  float10 in_ST0;
  float10 in_ST1;
  
  if (*(char *)(param_2 + 0xe) == '\x05') {
    uVar4 = (undefined2)
            CONCAT31((uint3)((byte)((ushort)*(undefined2 *)(unaff_EBP + -0xa4) >> 8) & 0xfe | 2),
                     0x3f);
  }
  else {
    uVar4 = 0x133f;
  }
  *(undefined2 *)(unaff_EBP + -0xa2) = uVar4;
  fVar1 = (float10)0;
  *(int *)(unaff_EBP + -0x94) = param_2;
  *(ushort *)(unaff_EBP + -0xa0) =
       (ushort)NAN(in_ST0) << 8 | (ushort)(in_ST0 < fVar1) << 9 | (ushort)(in_ST0 != fVar1) << 10 |
       (ushort)(in_ST0 == fVar1) << 0xe;
  *(undefined1 *)(unaff_EBP + -0x90) = 0;
  fVar1 = (float10)0;
  *(ushort *)(unaff_EBP + -0xa0) =
       (ushort)NAN(in_ST1) << 8 | (ushort)(in_ST1 < fVar1) << 9 | (ushort)(in_ST1 != fVar1) << 10 |
       (ushort)(in_ST1 == fVar1) << 0xe;
  bVar3 = (char)(*(char *)(unaff_EBP + -0x9f) << 1) >> 1;
  cVar2 = (char)(*(char *)(unaff_EBP + -0x9f) << 1) >> 1;
                    // WARNING: Could not recover jumptable at 0x00417171. Too many branches
                    // WARNING: Treating indirect jump as call
  (**(code **)(param_2 + (char)((&DAT_0043be2d)[(byte)(cVar2 << 1 | cVar2 < '\0') & 0xf] |
                               (&DAT_0043be2d)[(byte)((bVar3 & 7) << 1 | (char)bVar3 < '\0')] << 2)
              + 0x10))();
  return;
}



void FUN_00417178(void)

{
  return;
}



float10 FUN_00417186(void)

{
  return (float10)0;
}



float10 FUN_0041718b(void)

{
  return (float10)1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined1  [10] FUN_00417226(void)

{
  int unaff_EBP;
  undefined1 auVar1 [10];
  
  auVar1 = _DAT_0043be10;
  if (*(char *)(unaff_EBP + -0x90) < '\x01') {
    *(undefined1 *)(unaff_EBP + -0x90) = 1;
  }
  return auVar1;
}



undefined4 __cdecl
FUN_00417250(int param_1,int param_2,int param_3,uint param_4,undefined4 *param_5)

{
  double dVar1;
  int iVar2;
  undefined8 local_8;
  
  dVar1 = ABS((double)CONCAT44(param_2,param_1));
  if ((param_4 == 0x7ff00000) && (param_3 == 0)) {
    if (1.0 < dVar1) {
      param_5[1] = DAT_0043bfbc;
      *param_5 = DAT_0043bfb8;
      return 0;
    }
    if (dVar1 < 1.0) {
      *param_5 = 0;
      param_5[1] = 0;
      return 0;
    }
    param_5[1] = DAT_0043bfc4;
    *param_5 = DAT_0043bfc0;
    return 1;
  }
  if ((param_4 == 0xfff00000) && (param_3 == 0)) {
    if (1.0 < dVar1) {
      *param_5 = 0;
      param_5[1] = 0;
      return 0;
    }
    if (dVar1 < 1.0) {
      param_5[1] = DAT_0043bfbc;
      *param_5 = DAT_0043bfb8;
      return 0;
    }
    param_5[1] = DAT_0043bfc4;
    *param_5 = DAT_0043bfc0;
    return 1;
  }
  if ((param_2 != 0x7ff00000) || (param_1 != 0)) {
    if ((param_2 == -0x100000) && (param_1 == 0)) {
      iVar2 = FUN_004174a0(param_3,param_4);
      if (0.0 < (double)CONCAT44(param_4,param_3)) {
        if (iVar2 == 1) {
          local_8 = -(double)CONCAT44(DAT_0043bfbc,DAT_0043bfb8);
        }
        else {
          local_8 = (double)CONCAT44(DAT_0043bfbc,DAT_0043bfb8);
        }
        param_5[1] = local_8._4_4_;
        *param_5 = (undefined4)local_8;
        return 0;
      }
      if ((double)CONCAT44(param_4,param_3) < 0.0) {
        if (iVar2 == 1) {
          local_8._0_4_ = DAT_0043bfd8;
          local_8._4_4_ = DAT_0043bfdc;
        }
        else {
          local_8._0_4_ = 0;
          local_8._4_4_ = 0;
        }
        param_5[1] = local_8._4_4_;
        *param_5 = (undefined4)local_8;
        return 0;
      }
      *param_5 = 0;
      param_5[1] = 0x3ff00000;
    }
    return 0;
  }
  if (0.0 < (double)CONCAT44(param_4,param_3)) {
    param_5[1] = DAT_0043bfbc;
    *param_5 = DAT_0043bfb8;
    return 0;
  }
  *param_5 = 0;
  if ((double)CONCAT44(param_4,param_3) < 0.0) {
    param_5[1] = 0;
    return 0;
  }
  param_5[1] = 0x3ff00000;
  return 0;
}



undefined4 __cdecl FUN_004174a0(int param_1,uint param_2)

{
  double dVar1;
  uint uVar2;
  float10 fVar3;
  
  uVar2 = FUN_00419790(param_1,param_2);
  if ((uVar2 & 0x90) != 0) {
    return 0;
  }
  fVar3 = FUN_00419770((double)CONCAT44(param_2,param_1));
  if (fVar3 == (float10)(double)CONCAT44(param_2,param_1)) {
    dVar1 = (double)CONCAT44(param_2,param_1) * 0.5;
    fVar3 = FUN_00419770(dVar1);
    if (fVar3 == (float10)dVar1) {
      return 2;
    }
    return 1;
  }
  return 0;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

uint __cdecl
FUN_00417520(int param_1,uint param_2,ushort param_3,undefined4 param_4,uint param_5,uint param_6)

{
  float10 fVar1;
  uint uVar2;
  float10 fVar3;
  
  fVar1 = (float10)CONCAT28(param_3,CONCAT44(param_2,param_1));
  while (!CARRY4(param_2,param_2)) {
    if (param_1 == 0 && param_2 == 0) {
      return 0;
    }
    if ((param_3 & 0x7fff) != 0) {
      return param_3 & 0x7fff;
    }
    uVar2 = param_6 & 0x7fff;
    if (uVar2 == 0) {
      if (CARRY4(param_5,param_5)) {
        return param_5 * 2;
      }
    }
    else if ((uVar2 == 0x7fff) || (uVar2 = param_5 * 2, !CARRY4(param_5,param_5))) {
      return uVar2;
    }
    fVar3 = fVar1 * (float10)_DAT_0043be58;
    param_1 = SUB104(fVar3,0);
    param_3 = (ushort)((unkuint10)fVar3 >> 0x40);
    param_2 = (uint)((unkuint10)fVar3 >> 0x20);
  }
  uVar2 = param_2 * 2 ^ 0xe000000;
  if ((uVar2 & 0xe000000) != 0) {
    return uVar2;
  }
  uVar2 = param_2 * 2 >> 0x1c;
  if ((&DAT_0043be40)[uVar2] != '\0') {
    if (((param_3 & 0x7fff) != 0) && ((param_3 & 0x7fff) != 0x7fff)) {
      if ((param_6 & 0x7fff) != 1) {
        return param_6 & 0x7fff;
      }
      return 1;
    }
    return param_3 & 0x7fff;
  }
  return uVar2;
}



undefined4 FUN_00417d11(void)

{
  undefined4 in_EAX;
  unkbyte10 in_ST0;
  unkbyte10 in_ST1;
  undefined2 in_stack_ffffffe6;
  
  FUN_00417520((int)in_ST1,(uint)((unkuint10)in_ST1 >> 0x20),(ushort)((unkuint10)in_ST1 >> 0x40),
               (int)in_ST0,(uint)((unkuint10)in_ST0 >> 0x20),
               CONCAT22(in_stack_ffffffe6,(short)((unkuint10)in_ST0 >> 0x40)));
  return in_EAX;
}



// Library Function - Single Match
//  __87except
// 
// Library: Visual Studio 1998 Release

void __cdecl __87except(int param_1,int *param_2,ushort *param_3)

{
  bool bVar1;
  undefined3 extraout_var;
  int iVar2;
  uint uVar3;
  uint local_5c;
  uint local_58 [10];
  int local_30;
  int local_2c;
  uint local_20;
  
  local_5c = (uint)*param_3;
  switch(*param_2) {
  case 1:
  case 5:
    uVar3 = 8;
    break;
  case 2:
    uVar3 = 4;
    break;
  case 3:
    uVar3 = 0x11;
    break;
  case 4:
    uVar3 = 0x12;
    break;
  case 7:
    *param_2 = 1;
  default:
    uVar3 = 0;
    break;
  case 8:
    uVar3 = 0x10;
  }
  if (uVar3 != 0) {
    bVar1 = __handle_exc(uVar3,(double *)(param_2 + 6),local_5c);
    if (CONCAT31(extraout_var,bVar1) == 0) {
      if (((param_1 == 0x10) || (param_1 == 0x16)) || (param_1 == 0x1d)) {
        local_2c = param_2[5];
        local_20 = local_20 & 0xffffffe3 | 3;
        local_30 = param_2[4];
      }
      else {
        local_20 = local_20 & 0xfffffffe;
      }
      __raise_exc(local_58,&local_5c,uVar3,param_1,(uint *)(param_2 + 2),(uint *)(param_2 + 6));
    }
  }
  __ctrlfp();
  iVar2 = 0;
  if ((*param_2 != 8) && (DAT_0043c0b8 == 0)) {
    iVar2 = FUN_00419ea0();
  }
  if (iVar2 == 0) {
    __set_errno(*param_2);
  }
  return;
}



// Library Function - Single Match
//  ___crtGetStringTypeA
// 
// Library: Visual Studio 1998 Release

BOOL __cdecl
___crtGetStringTypeA
          (_locale_t _Plocinfo,DWORD _DWInfoType,LPCSTR _LpSrcStr,int _CchSrc,LPWORD _LpCharType,
          int _Code_page,BOOL _BError)

{
  BOOL BVar1;
  size_t _Size;
  int cchSrc;
  int iVar2;
  LPCWSTR lpWideCharStr;
  WORD local_2;
  
  iVar2 = DAT_0043bfa4;
  if (DAT_0043bfa4 == 0) {
    BVar1 = GetStringTypeA(0,1,"",1,&local_2);
    if (BVar1 == 0) {
      BVar1 = GetStringTypeW(1,L"",1,&local_2);
      if (BVar1 == 0) {
        return 0;
      }
      iVar2 = 1;
    }
    else {
      iVar2 = 2;
    }
  }
  DAT_0043bfa4 = iVar2;
  if (iVar2 != 2) {
    if (iVar2 == 1) {
      BVar1 = 0;
      lpWideCharStr = (LPCWSTR)0x0;
      if (_LpCharType == (LPWORD)0x0) {
        _LpCharType = DAT_0043bde8;
      }
      _Size = MultiByteToWideChar((UINT)_LpCharType,9,(LPCSTR)_DWInfoType,(int)_LpSrcStr,(LPWSTR)0x0
                                  ,0);
      iVar2 = BVar1;
      if (((_Size != 0) &&
          (lpWideCharStr = (LPCWSTR)_calloc(2,_Size), lpWideCharStr != (LPCWSTR)0x0)) &&
         (cchSrc = MultiByteToWideChar((UINT)_LpCharType,1,(LPCSTR)_DWInfoType,(int)_LpSrcStr,
                                       lpWideCharStr,_Size), cchSrc != 0)) {
        iVar2 = GetStringTypeW((DWORD)_Plocinfo,lpWideCharStr,cchSrc,(LPWORD)_CchSrc);
      }
      _free(lpWideCharStr);
    }
    return iVar2;
  }
  if (_Code_page == 0) {
    _Code_page = DAT_0043bdd8;
  }
  BVar1 = GetStringTypeA(_Code_page,(DWORD)_Plocinfo,(LPCSTR)_DWInfoType,(int)_LpSrcStr,
                         (LPWORD)_CchSrc);
  return BVar1;
}



// Library Function - Single Match
//  ___crtLCMapStringA
// 
// Library: Visual Studio 1998 Release

int __cdecl
___crtLCMapStringA(_locale_t _Plocinfo,LPCWSTR _LocaleName,DWORD _DwMapFlag,LPCSTR _LpSrcStr,
                  int _CchSrc,LPSTR _LpDestStr,int _CchDest,int _Code_page,BOOL _BError)

{
  int iVar1;
  LPCWSTR lpWideCharStr;
  int iVar2;
  LPCWSTR lpDestStr;
  
  if (DAT_0043bfac == 0) {
    iVar1 = LCMapStringA(0,0x100,"",1,(LPSTR)0x0,0);
    if (iVar1 == 0) {
      iVar1 = LCMapStringW(0,0x100,L"",1,(LPWSTR)0x0,0);
      if (iVar1 == 0) {
        return 0;
      }
      DAT_0043bfac = 1;
    }
    else {
      DAT_0043bfac = 2;
    }
  }
  if (0 < (int)_LpSrcStr) {
    _LpSrcStr = (LPCSTR)_strncnt((char *)_DwMapFlag,(size_t)_LpSrcStr);
  }
  if (DAT_0043bfac == 2) {
    iVar1 = LCMapStringA((LCID)_Plocinfo,(DWORD)_LocaleName,(LPCSTR)_DwMapFlag,(int)_LpSrcStr,
                         (LPSTR)_CchSrc,(int)_LpDestStr);
    return iVar1;
  }
  if (DAT_0043bfac != 1) {
    return DAT_0043bfac;
  }
  lpDestStr = (LPCWSTR)0x0;
  if (_CchDest == 0) {
    _CchDest = DAT_0043bde8;
  }
  iVar1 = MultiByteToWideChar(_CchDest,9,(LPCSTR)_DwMapFlag,(int)_LpSrcStr,(LPWSTR)0x0,0);
  if (iVar1 == 0) {
    return 0;
  }
  lpWideCharStr = (LPCWSTR)_malloc(iVar1 * 2);
  if (lpWideCharStr == (LPCWSTR)0x0) {
    return 0;
  }
  iVar2 = MultiByteToWideChar(_CchDest,1,(LPCSTR)_DwMapFlag,(int)_LpSrcStr,lpWideCharStr,iVar1);
  if ((iVar2 != 0) &&
     (iVar2 = LCMapStringW((LCID)_Plocinfo,(DWORD)_LocaleName,lpWideCharStr,iVar1,(LPWSTR)0x0,0),
     iVar2 != 0)) {
    if (((uint)_LocaleName & 0x400) == 0) {
      lpDestStr = (LPCWSTR)_malloc(iVar2 * 2);
      if ((lpDestStr == (LPCWSTR)0x0) ||
         (iVar1 = LCMapStringW((LCID)_Plocinfo,(DWORD)_LocaleName,lpWideCharStr,iVar1,lpDestStr,
                               iVar2), iVar1 == 0)) goto LAB_0041867b;
      if (_LpDestStr == (LPSTR)0x0) {
        iVar2 = WideCharToMultiByte(_CchDest,0x220,lpDestStr,iVar2,(LPSTR)0x0,0,(LPCSTR)0x0,
                                    (LPBOOL)0x0);
        iVar1 = iVar2;
      }
      else {
        iVar2 = WideCharToMultiByte(_CchDest,0x220,lpDestStr,iVar2,(LPSTR)_CchSrc,(int)_LpDestStr,
                                    (LPCSTR)0x0,(LPBOOL)0x0);
        iVar1 = iVar2;
      }
    }
    else {
      if (_LpDestStr == (LPSTR)0x0) goto LAB_00418712;
      if ((int)_LpDestStr < iVar2) goto LAB_0041867b;
      iVar1 = LCMapStringW((LCID)_Plocinfo,(DWORD)_LocaleName,lpWideCharStr,iVar1,(LPWSTR)_CchSrc,
                           (int)_LpDestStr);
    }
    if (iVar1 != 0) {
LAB_00418712:
      _free(lpWideCharStr);
      _free(lpDestStr);
      return iVar2;
    }
  }
LAB_0041867b:
  _free(lpWideCharStr);
  _free(lpDestStr);
  return 0;
}



// Library Function - Single Match
//  _strncnt
// 
// Library: Visual Studio 1998 Release

size_t __cdecl _strncnt(char *_String,size_t _Cnt)

{
  size_t sVar1;
  char *pcVar2;
  
  pcVar2 = _String;
  sVar1 = _Cnt;
  if (_Cnt != 0) {
    do {
      sVar1 = sVar1 - 1;
      if (*pcVar2 == '\0') goto LAB_00418755;
      pcVar2 = pcVar2 + 1;
    } while (sVar1 != 0);
  }
  if (*pcVar2 == '\0') {
LAB_00418755:
    _Cnt = (int)pcVar2 - (int)_String;
  }
  return _Cnt;
}



// Library Function - Single Match
//  ___addl
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl ___addl(uint param_1,uint param_2,uint *param_3)

{
  uint uVar1;
  undefined4 uVar2;
  
  uVar2 = 0;
  uVar1 = param_1 + param_2;
  if ((uVar1 < param_1) || (uVar1 < param_2)) {
    uVar2 = 1;
  }
  *param_3 = uVar1;
  return uVar2;
}



// Library Function - Single Match
//  ___add_12
// 
// Library: Visual Studio 1998 Release

void __cdecl ___add_12(uint *param_1,uint *param_2)

{
  int iVar1;
  
  iVar1 = ___addl(*param_1,*param_2,param_1);
  if (iVar1 != 0) {
    iVar1 = ___addl(param_1[1],1,param_1 + 1);
    if (iVar1 != 0) {
      param_1[2] = param_1[2] + 1;
    }
  }
  iVar1 = ___addl(param_1[1],param_2[1],param_1 + 1);
  if (iVar1 != 0) {
    param_1[2] = param_1[2] + 1;
  }
  ___addl(param_1[2],param_2[2],param_1 + 2);
  return;
}



// Library Function - Single Match
//  ___shl_12
// 
// Library: Visual Studio 1998 Release

void __cdecl ___shl_12(uint *param_1)

{
  uint uVar1;
  uint uVar2;
  
  uVar1 = *param_1;
  uVar2 = param_1[1];
  *param_1 = uVar1 * 2;
  param_1[1] = uVar2 * 2 | uVar1 >> 0x1f;
  param_1[2] = param_1[2] * 2 | uVar2 >> 0x1f;
  return;
}



// Library Function - Single Match
//  ___shr_12
// 
// Library: Visual Studio 1998 Release

void __cdecl ___shr_12(uint *param_1)

{
  uint uVar1;
  uint uVar2;
  
  uVar1 = param_1[2];
  uVar2 = param_1[1];
  param_1[2] = uVar1 >> 1;
  param_1[1] = uVar2 >> 1 | uVar1 << 0x1f;
  *param_1 = *param_1 >> 1 | uVar2 << 0x1f;
  return;
}



// Library Function - Single Match
//  ___mtold12
// 
// Library: Visual Studio 1998 Release

void __cdecl ___mtold12(char *param_1,int param_2,uint *param_3)

{
  byte bVar1;
  uint uVar2;
  short sVar3;
  uint local_c;
  uint local_8;
  uint local_4;
  
  sVar3 = 0x404e;
  *param_3 = 0;
  param_3[1] = 0;
  param_3[2] = 0;
  if (param_2 != 0) {
    do {
      local_c = *param_3;
      local_8 = param_3[1];
      local_4 = param_3[2];
      ___shl_12(param_3);
      param_2 = param_2 + -1;
      ___shl_12(param_3);
      ___add_12(param_3,&local_c);
      ___shl_12(param_3);
      local_c = (uint)*param_1;
      local_8 = 0;
      local_4 = 0;
      ___add_12(param_3,&local_c);
      param_1 = param_1 + 1;
    } while (param_2 != 0);
  }
  uVar2 = param_3[2];
  while (uVar2 == 0) {
    sVar3 = sVar3 + -0x10;
    uVar2 = param_3[1] >> 0x10;
    param_3[2] = uVar2;
    param_3[1] = param_3[1] << 0x10 | *param_3 >> 0x10;
    *param_3 = *param_3 << 0x10;
  }
  bVar1 = *(byte *)((int)param_3 + 9);
  while ((bVar1 & 0x80) == 0) {
    sVar3 = sVar3 + -1;
    ___shl_12(param_3);
    bVar1 = *(byte *)((int)param_3 + 9);
  }
  *(short *)((int)param_3 + 10) = sVar3;
  return;
}



// Library Function - Single Match
//  ___strgtold12
// 
// Library: Visual Studio 1998 Release

uint __cdecl
___strgtold12(_LDBL12 *pld12,char **p_end_ptr,char *str,int mult12,int scale,int decpt,
             int implicit_E)

{
  char cVar1;
  bool bVar2;
  bool bVar3;
  bool bVar4;
  bool bVar5;
  bool bVar6;
  int iVar7;
  byte bVar8;
  char *pcVar9;
  char *pcVar10;
  byte *pbVar11;
  int iVar12;
  uint uVar13;
  ushort local_52;
  uint local_50;
  byte *local_4c;
  int local_48;
  uint local_40;
  int local_3c;
  undefined4 local_28 [5];
  char local_11;
  ushort local_c;
  undefined4 local_a;
  undefined4 local_6;
  ushort local_2;
  
  local_3c = 1;
  pcVar10 = (char *)local_28;
  iVar12 = 0;
  local_52 = 0;
  local_50 = 0;
  bVar2 = false;
  bVar4 = false;
  bVar3 = false;
  bVar5 = false;
  iVar7 = 0;
  bVar6 = false;
  local_48 = 0;
  local_40 = 0;
  local_4c = (byte *)str;
  for (; (((bVar8 = *str, bVar8 == 0x20 || (bVar8 == 9)) || (bVar8 == 10)) || (bVar8 == 0xd));
      str = (char *)((byte *)str + 1)) {
  }
  do {
    bVar8 = *str;
    pbVar11 = (byte *)str + 1;
    switch(iVar7) {
    case 0:
      if (((char)bVar8 < '1') || ('9' < (char)bVar8)) {
        if (DAT_0043bb84 == bVar8) {
          iVar7 = 5;
        }
        else if (bVar8 == 0x2b) {
          local_52 = 0;
          iVar7 = 2;
        }
        else if (bVar8 == 0x2d) {
          local_52 = 0x8000;
          iVar7 = 2;
        }
        else {
          if (bVar8 != 0x30) goto switchD_00418c20_caseD_2c;
          iVar7 = 1;
        }
        break;
      }
      iVar7 = 3;
      goto LAB_00418e24;
    case 1:
      bVar2 = true;
      if (('0' < (char)bVar8) && ((char)bVar8 < ':')) {
        iVar7 = 3;
        goto LAB_00418e24;
      }
      if (DAT_0043bb84 == bVar8) {
        iVar7 = 4;
      }
      else {
        switch(bVar8) {
        case 0x2b:
        case 0x2d:
          iVar7 = 0xb;
          pbVar11 = (byte *)str;
          break;
        default:
          goto switchD_00418c20_caseD_2c;
        case 0x30:
          iVar7 = 1;
          break;
        case 0x44:
        case 0x45:
        case 100:
        case 0x65:
          iVar7 = 6;
        }
      }
      break;
    case 2:
      if (('0' < (char)bVar8) && ((char)bVar8 < ':')) {
        iVar7 = 3;
        goto LAB_00418e24;
      }
      if (DAT_0043bb84 == bVar8) {
        iVar7 = 5;
      }
      else if (bVar8 == 0x30) {
        iVar7 = 1;
      }
      else {
        iVar7 = 10;
        pbVar11 = local_4c;
      }
      break;
    case 3:
      bVar2 = true;
      while( true ) {
        if (DAT_0043bb80 < 2) {
          uVar13 = *(ushort *)(PTR_DAT_0043bb90 + (uint)bVar8 * 2) & 4;
        }
        else {
          uVar13 = __isctype((uint)bVar8,4);
        }
        if (uVar13 == 0) break;
        if (local_50 < 0x19) {
          local_50 = local_50 + 1;
          *pcVar10 = bVar8 - 0x30;
          bVar8 = *pbVar11;
          pcVar10 = pcVar10 + 1;
          pbVar11 = pbVar11 + 1;
        }
        else {
          bVar8 = *pbVar11;
          pbVar11 = pbVar11 + 1;
          local_48 = local_48 + 1;
        }
      }
      if (DAT_0043bb84 == bVar8) {
        iVar7 = 4;
      }
      else {
        switch(bVar8) {
        case 0x2b:
        case 0x2d:
          iVar7 = 0xb;
          pbVar11 = pbVar11 + -1;
          break;
        default:
          goto switchD_00418c20_caseD_2c;
        case 0x44:
        case 0x45:
        case 100:
        case 0x65:
          iVar7 = 6;
        }
      }
      break;
    case 4:
      bVar2 = true;
      bVar4 = true;
      if (local_50 == 0) {
        while (bVar8 == 0x30) {
          local_48 = local_48 + -1;
          bVar8 = *pbVar11;
          pbVar11 = pbVar11 + 1;
        }
      }
      while( true ) {
        if (DAT_0043bb80 < 2) {
          uVar13 = *(ushort *)(PTR_DAT_0043bb90 + (uint)bVar8 * 2) & 4;
        }
        else {
          uVar13 = __isctype((uint)bVar8,4);
        }
        if (uVar13 == 0) break;
        pcVar9 = pcVar10;
        if (local_50 < 0x19) {
          pcVar9 = pcVar10 + 1;
          local_50 = local_50 + 1;
          local_48 = local_48 + -1;
          *pcVar10 = bVar8 - 0x30;
        }
        bVar8 = *pbVar11;
        pbVar11 = pbVar11 + 1;
        pcVar10 = pcVar9;
      }
      switch(bVar8) {
      case 0x2b:
      case 0x2d:
        iVar7 = 0xb;
        pbVar11 = pbVar11 + -1;
        break;
      default:
        goto switchD_00418c20_caseD_2c;
      case 0x44:
      case 0x45:
      case 100:
      case 0x65:
        iVar7 = 6;
      }
      break;
    case 5:
      bVar4 = true;
      if (DAT_0043bb80 < 2) {
        uVar13 = *(ushort *)(PTR_DAT_0043bb90 + (uint)bVar8 * 2) & 4;
      }
      else {
        uVar13 = __isctype((uint)bVar8,4);
      }
      if (uVar13 != 0) {
        iVar7 = 4;
        goto LAB_00418e24;
      }
      iVar7 = 10;
      pbVar11 = local_4c;
      break;
    case 6:
      local_4c = (byte *)str + -1;
      if (('0' < (char)bVar8) && ((char)bVar8 < ':')) {
        iVar7 = 9;
        goto LAB_00418e24;
      }
      if (bVar8 == 0x2b) {
        iVar7 = 7;
      }
      else if (bVar8 == 0x2d) {
        local_3c = -1;
        iVar7 = 7;
      }
      else if (bVar8 == 0x30) {
        iVar7 = 8;
      }
      else {
        iVar7 = 10;
        pbVar11 = local_4c;
      }
      break;
    case 7:
      if (('0' < (char)bVar8) && ((char)bVar8 < ':')) {
        iVar7 = 9;
        goto LAB_00418e24;
      }
      if (bVar8 == 0x30) {
        iVar7 = 8;
      }
      else {
        iVar7 = 10;
        pbVar11 = local_4c;
      }
      break;
    case 8:
      bVar3 = true;
      while (bVar8 == 0x30) {
        bVar8 = *pbVar11;
        pbVar11 = pbVar11 + 1;
      }
      if (('0' < (char)bVar8) && ((char)bVar8 < ':')) {
        iVar7 = 9;
        goto LAB_00418e24;
      }
      goto switchD_00418c20_caseD_2c;
    case 9:
      bVar3 = true;
      iVar12 = 0;
      while( true ) {
        if (DAT_0043bb80 < 2) {
          uVar13 = *(ushort *)(PTR_DAT_0043bb90 + (uint)bVar8 * 2) & 4;
        }
        else {
          uVar13 = __isctype((uint)bVar8,4);
        }
        if (uVar13 == 0) goto LAB_00418daa;
        iVar12 = (char)bVar8 + -0x30 + iVar12 * 10;
        if (0x1450 < iVar12) break;
        bVar8 = *pbVar11;
        pbVar11 = pbVar11 + 1;
      }
      iVar12 = 0x1451;
LAB_00418daa:
      while( true ) {
        if (DAT_0043bb80 < 2) {
          uVar13 = *(ushort *)(PTR_DAT_0043bb90 + (uint)bVar8 * 2) & 4;
        }
        else {
          uVar13 = __isctype((uint)bVar8,4);
        }
        if (uVar13 == 0) break;
        bVar8 = *pbVar11;
        pbVar11 = pbVar11 + 1;
      }
switchD_00418c20_caseD_2c:
      iVar7 = 10;
LAB_00418e24:
      pbVar11 = pbVar11 + -1;
      break;
    case 0xb:
      if (implicit_E == 0) goto switchD_00418c20_caseD_2c;
      local_4c = (byte *)str;
      if (bVar8 == 0x2b) {
        iVar7 = 7;
      }
      else if (bVar8 == 0x2d) {
        local_3c = -1;
        iVar7 = 7;
      }
      else {
        iVar7 = 10;
        pbVar11 = (byte *)str;
      }
    }
    str = (char *)pbVar11;
  } while (iVar7 != 10);
  *p_end_ptr = (char *)pbVar11;
  if (bVar2) {
    if (0x18 < local_50) {
      if ('\x04' < local_11) {
        local_11 = local_11 + '\x01';
      }
      pcVar10 = pcVar10 + -1;
      local_48 = local_48 + 1;
      local_50 = 0x18;
    }
    if (local_50 == 0) {
      local_c = 0;
      local_6 = 0;
      local_2 = 0;
      local_28[0] = 0;
      goto LAB_00418f22;
    }
    pcVar10 = pcVar10 + -1;
    cVar1 = *pcVar10;
    while (cVar1 == '\0') {
      pcVar10 = pcVar10 + -1;
      local_50 = local_50 - 1;
      local_48 = local_48 + 1;
      cVar1 = *pcVar10;
    }
    ___mtold12((char *)local_28,local_50,(uint *)&local_c);
    if (local_3c < 0) {
      iVar12 = -iVar12;
    }
    uVar13 = iVar12 + local_48;
    if (!bVar3) {
      uVar13 = uVar13 + scale;
    }
    if (!bVar4) {
      uVar13 = uVar13 - decpt;
    }
    if ((int)uVar13 < 0x1451) {
      if (-0x1451 < (int)uVar13) {
        ___multtenpow12((int *)&local_c,uVar13,mult12);
        local_28[0] = local_a;
        goto LAB_00418f22;
      }
      bVar6 = true;
    }
    else {
      bVar5 = true;
    }
  }
  local_6 = local_28[0];
  local_c = (ushort)local_28[0];
  local_2 = (ushort)local_28[0];
LAB_00418f22:
  if (bVar2) {
    if (bVar5) {
      local_2 = 0x7fff;
      local_6 = 0x80000000;
      local_c = 0;
      local_28[0] = 0;
      local_40 = 2;
    }
    else if (bVar6) {
      local_c = 0;
      local_6 = 0;
      local_2 = 0;
      local_28[0] = 0;
      local_40 = 1;
    }
  }
  else {
    local_c = 0;
    local_6 = 0;
    local_2 = 0;
    local_28[0] = 0;
    local_40 = 4;
  }
  *(ushort *)pld12->ld12 = local_c;
  *(undefined4 *)(pld12->ld12 + 2) = local_28[0];
  *(ushort *)(pld12->ld12 + 10) = local_2 | local_52;
  *(undefined4 *)(pld12->ld12 + 6) = local_6;
  return local_40;
}



// Library Function - Single Match
//  _$I10_OUTPUT
// 
// Library: Visual Studio 1998 Release

undefined4 __cdecl
_I10_OUTPUT(int param_1,uint param_2,ushort param_3,int param_4,byte param_5,short *param_6)

{
  short *psVar1;
  ushort uVar2;
  char cVar3;
  int iVar4;
  uint uVar5;
  short *psVar6;
  short *psVar7;
  int iVar8;
  short sVar9;
  undefined2 local_28;
  undefined4 uStack_26;
  undefined4 uStack_22;
  undefined1 local_1e;
  char cStack_1d;
  undefined4 local_1c;
  undefined1 local_18;
  undefined1 local_17;
  undefined1 local_16;
  undefined1 local_15;
  undefined1 local_14;
  undefined1 local_13;
  undefined1 local_12;
  undefined1 local_11;
  undefined1 local_10;
  undefined1 local_f;
  undefined1 local_e;
  undefined1 local_d;
  uint local_c;
  undefined4 local_8;
  undefined4 local_4;
  
  local_18 = 0xcc;
  local_17 = 0xcc;
  local_16 = 0xcc;
  local_15 = 0xcc;
  local_14 = 0xcc;
  local_13 = 0xcc;
  local_12 = 0xcc;
  local_11 = 0xcc;
  local_10 = 0xcc;
  local_f = 0xcc;
  local_e = 0xfb;
  local_d = 0x3f;
  local_1c = 1;
  uVar2 = param_3 & 0x7fff;
  if ((param_3 & 0x8000) == 0) {
    *(undefined1 *)(param_6 + 1) = 0x20;
  }
  else {
    *(undefined1 *)(param_6 + 1) = 0x2d;
  }
  if (((uVar2 == 0) && (param_2 == 0)) && (param_1 == 0)) {
    *(undefined1 *)(param_6 + 1) = 0x20;
    *param_6 = 0;
    *(undefined1 *)((int)param_6 + 3) = 1;
    *(undefined1 *)(param_6 + 2) = 0x30;
    *(undefined1 *)((int)param_6 + 5) = 0;
    return 1;
  }
  if (uVar2 == 0x7fff) {
    *param_6 = 1;
    if (((param_2 != 0x80000000) || (param_1 != 0)) && ((param_2 & 0x40000000) == 0)) {
      param_6[2] = 0x2331;
      param_6[3] = 0x4e53;
      param_6[4] = 0x4e41;
      *(undefined1 *)(param_6 + 5) = 0;
      *(undefined1 *)((int)param_6 + 3) = 6;
      return 0;
    }
    if ((((param_3 & 0x8000) != 0) && (param_2 == 0xc0000000)) && (param_1 == 0)) {
      param_6[2] = 0x2331;
      param_6[3] = 0x4e49;
      param_6[4] = 0x44;
      *(undefined1 *)((int)param_6 + 3) = 5;
      return 0;
    }
    if ((param_2 == 0x80000000) && (param_1 == 0)) {
      param_6[2] = 0x2331;
      param_6[3] = 0x4e49;
      param_6[4] = 0x46;
      *(undefined1 *)((int)param_6 + 3) = 5;
      return 0;
    }
    param_6[2] = 0x2331;
    param_6[3] = 0x4e51;
    param_6[4] = 0x4e41;
    *(undefined1 *)(param_6 + 5) = 0;
    *(undefined1 *)((int)param_6 + 3) = 6;
    return 0;
  }
  local_1e = (undefined1)uVar2;
  cStack_1d = (char)(uVar2 >> 8);
  sVar9 = (short)(((uint)(uVar2 >> 8) + (param_2 >> 0x18) * 2) * 0x4d + (uint)uVar2 * 0x4d10 +
                  -0x134312f4 >> 0x10);
  local_28 = 0;
  uStack_22 = param_2;
  uStack_26 = param_1;
  ___multtenpow12((int *)&local_28,-(int)sVar9,1);
  if (0x3ffe < CONCAT11(cStack_1d,local_1e)) {
    sVar9 = sVar9 + 1;
    ___ld12mul((int *)&local_28,(int *)&local_18);
  }
  *param_6 = sVar9;
  if (((param_5 & 1) != 0) && (param_4 = param_4 + sVar9, param_4 < 1)) {
    *(undefined1 *)(param_6 + 1) = 0x20;
    *param_6 = 0;
    *(undefined1 *)((int)param_6 + 3) = 1;
    *(undefined1 *)(param_6 + 2) = 0x30;
    *(undefined1 *)((int)param_6 + 5) = 0;
    return 1;
  }
  if (0x15 < param_4) {
    param_4 = 0x15;
  }
  iVar8 = 8;
  uVar2 = CONCAT11(cStack_1d,local_1e);
  local_1e = 0;
  cStack_1d = '\0';
  iVar4 = uVar2 - 0x3ffe;
  do {
    ___shl_12((uint *)&local_28);
    iVar8 = iVar8 + -1;
  } while (iVar8 != 0);
  if (iVar4 < 0) {
    for (uVar5 = -iVar4 & 0xff; uVar5 != 0; uVar5 = uVar5 - 1) {
      ___shr_12((uint *)&local_28);
    }
  }
  psVar1 = param_6 + 2;
  iVar8 = param_4 + 1;
  psVar7 = psVar1;
  psVar6 = psVar1;
  iVar4 = uStack_26;
  uVar5 = uStack_22;
  if (0 < iVar8) {
    do {
      uStack_22._2_2_ = (undefined2)(uVar5 >> 0x10);
      uStack_22._0_2_ = (undefined2)uVar5;
      uStack_26._2_2_ = (undefined2)((uint)iVar4 >> 0x10);
      uStack_26._0_2_ = (undefined2)iVar4;
      psVar6 = (short *)((int)psVar7 + 1);
      local_c = CONCAT22((undefined2)uStack_26,local_28);
      local_8 = CONCAT22((undefined2)uStack_22,uStack_26._2_2_);
      local_4 = CONCAT13(cStack_1d,CONCAT12(local_1e,uStack_22._2_2_));
      uStack_26 = iVar4;
      uStack_22 = uVar5;
      ___shl_12((uint *)&local_28);
      ___shl_12((uint *)&local_28);
      ___add_12((uint *)&local_28,&local_c);
      ___shl_12((uint *)&local_28);
      iVar8 = iVar8 + -1;
      *(char *)psVar7 = cStack_1d + '0';
      cStack_1d = '\0';
      psVar7 = psVar6;
      iVar4 = uStack_26;
      uVar5 = uStack_22;
    } while (iVar8 != 0);
  }
  psVar7 = psVar6 + -1;
  if (*(char *)((int)psVar6 + -1) < '5') {
    if (psVar7 < psVar1) {
LAB_00419439:
      *param_6 = 0;
      *(char *)psVar1 = '0';
      *(undefined1 *)(param_6 + 1) = 0x20;
      *(undefined1 *)((int)param_6 + 3) = 1;
      *(undefined1 *)((int)param_6 + 5) = 0;
      return 1;
    }
    do {
      if ((char)*psVar7 != '0') break;
      psVar7 = (short *)((int)psVar7 + -1);
    } while (psVar1 <= psVar7);
    if (psVar7 < psVar1) goto LAB_00419439;
    goto LAB_0041940b;
  }
  if (psVar7 < psVar1) {
LAB_00419405:
    *param_6 = *param_6 + 1;
    psVar7 = (short *)((int)psVar7 + 1);
  }
  else {
    do {
      if ((char)*psVar7 != '9') break;
      *(char *)psVar7 = '0';
      psVar7 = (short *)((int)psVar7 + -1);
    } while (psVar1 <= psVar7);
    if (psVar7 < psVar1) goto LAB_00419405;
  }
  *(char *)psVar7 = (char)*psVar7 + '\x01';
LAB_0041940b:
  cVar3 = ((char)psVar7 - (char)param_6) + -3;
  *(char *)((int)param_6 + 3) = cVar3;
  *(undefined1 *)(cVar3 + 4 + (int)param_6) = 0;
  return local_1c;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __chsize
// 
// Library: Visual Studio 1998 Release

int __cdecl __chsize(int _FileHandle,long _Size)

{
  long _Offset;
  long lVar1;
  uint _MaxCharCount;
  int iVar2;
  HANDLE hFile;
  BOOL BVar3;
  int iVar4;
  int iVar5;
  uint uVar6;
  int *piVar7;
  uint in_stack_00001008;
  int in_stack_0000100c;
  
  FUN_0041a340();
  iVar5 = 0;
  if ((DAT_00454680 <= in_stack_00001008) ||
     ((*(byte *)(*(int *)((int)&DAT_00454580 + ((int)(in_stack_00001008 & 0xffffffe7) >> 3)) + 4 +
                (in_stack_00001008 & 0x1f) * 8) & 1) == 0)) {
    _DAT_0043aa58 = 9;
    return -1;
  }
  _Offset = __lseek(in_stack_00001008,0,1);
  if ((_Offset == -1) || (lVar1 = __lseek(in_stack_00001008,0,2), lVar1 == -1)) {
    return -1;
  }
  uVar6 = in_stack_0000100c - lVar1;
  if ((int)uVar6 < 1) {
    if ((int)uVar6 < 0) {
      __lseek(in_stack_00001008,in_stack_0000100c,0);
      hFile = (HANDLE)__get_osfhandle(in_stack_00001008);
      BVar3 = SetEndOfFile(hFile);
      iVar5 = -(uint)(BVar3 == 0);
      if (iVar5 == -1) {
        _DAT_0043aa58 = 0xd;
        DAT_0043aa5c = GetLastError();
      }
    }
  }
  else {
    piVar7 = &_FileHandle;
    for (iVar4 = 0x400; iVar4 != 0; iVar4 = iVar4 + -1) {
      *piVar7 = 0;
      piVar7 = piVar7 + 1;
    }
    iVar4 = __setmode(in_stack_00001008,0x8000);
    do {
      _MaxCharCount = 0x1000;
      if ((int)uVar6 < 0x1000) {
        _MaxCharCount = uVar6;
      }
      iVar2 = __write(in_stack_00001008,&_FileHandle,_MaxCharCount);
      if (iVar2 == -1) {
        if (DAT_0043aa5c == 5) {
          _DAT_0043aa58 = 0xd;
        }
        iVar5 = -1;
        break;
      }
      uVar6 = uVar6 - iVar2;
    } while (0 < (int)uVar6);
    __setmode(in_stack_00001008,iVar4);
  }
  __lseek(in_stack_00001008,_Offset,0);
  return iVar5;
}



// Library Function - Single Match
//  __set_exp
// 
// Library: Visual Studio 1998 Release

float10 __cdecl __set_exp(undefined4 param_1,undefined4 param_2,short param_3)

{
  undefined2 uStack_4;
  
  uStack_4 = (undefined2)param_2;
  return (float10)(double)CONCAT26((param_3 + 0x3fe) * 0x10 | param_2._2_2_ & 0x800f,
                                   CONCAT24(uStack_4,param_1));
}



undefined4 __cdecl FUN_00419610(int param_1,uint param_2)

{
  if ((param_2 == 0x7ff00000) && (param_1 == 0)) {
    return 1;
  }
  if ((param_2 == 0xfff00000) && (param_1 == 0)) {
    return 2;
  }
  if ((param_2._2_2_ & 0x7ff8) == 0x7ff8) {
    return 3;
  }
  if (((param_2._2_2_ & 0x7ff8) == 0x7ff0) && (((param_2 & 0x7ffff) != 0 || (param_1 != 0)))) {
    return 4;
  }
  return 0;
}



// Library Function - Single Match
//  __decomp
// 
// Library: Visual Studio 1998 Release

float10 __cdecl __decomp(uint param_1,uint param_2,int *param_3)

{
  ushort uVar1;
  int iVar2;
  double dVar3;
  byte bVar4;
  int iVar5;
  float10 fVar6;
  double local_8;
  
  if ((param_2 & 0x7fffffff) == 0 && param_1 == 0) {
    iVar5 = 0;
    local_8 = 0.0;
  }
  else if (((param_2 & 0x7ff00000) == 0) && (((param_2 & 0xfffff) != 0 || (param_1 != 0)))) {
    dVar3 = (double)CONCAT17(param_2._3_1_,CONCAT16(param_2._2_1_,CONCAT24((ushort)param_2,param_1))
                            );
    iVar5 = -0x3fd;
    if ((param_2 & 0x100000) == 0) {
      do {
        bVar4 = param_2._2_1_;
        iVar2 = CONCAT13(param_2._3_1_,CONCAT12(param_2._2_1_,(ushort)param_2)) << 1;
        param_2._0_2_ = (ushort)iVar2;
        param_2._2_1_ = (byte)((uint)iVar2 >> 0x10);
        param_2._3_1_ = (byte)((uint)iVar2 >> 0x18);
        if ((param_1 & 0x80000000) != 0) {
          param_2._0_2_ = (ushort)param_2 | 1;
        }
        iVar5 = iVar5 + -1;
        param_1 = param_1 << 1;
      } while ((bVar4 & 8) == 0);
    }
    uVar1 = CONCAT11(param_2._3_1_,param_2._2_1_) & 0xffef;
    param_2._2_1_ = (byte)uVar1;
    param_2._3_1_ = (byte)(uVar1 >> 8);
    if (dVar3 < 0.0) {
      param_2._3_1_ = param_2._3_1_ | 0x80;
    }
    fVar6 = __set_exp(param_1,CONCAT13(param_2._3_1_,CONCAT12(param_2._2_1_,(ushort)param_2)),0);
    local_8 = (double)fVar6;
  }
  else {
    iVar5 = (short)(((ushort)(param_2 >> 0x10) & 0x7ff0) >> 4) + -0x3fe;
    fVar6 = __set_exp(param_1,param_2,0);
    local_8 = (double)fVar6;
  }
  *param_3 = iVar5;
  return (float10)local_8;
}



float10 __cdecl FUN_00419770(double param_1)

{
  return (float10)ROUND(param_1);
}



int __cdecl FUN_00419790(int param_1,uint param_2)

{
  int iVar1;
  
  if ((param_2._2_2_ & 0x7ff0) != 0x7ff0) {
    if ((param_2 & 0x7fffffff) == 0 && param_1 == 0) {
      return (-(uint)((param_2 & 0x80000000) == 0) & 0x20) + 0x20;
    }
    if (((param_2 & 0x7ff00000) == 0) && (((param_2 & 0xfffff) != 0 || (param_1 != 0)))) {
      return (-(uint)((param_2 & 0x80000000) == 0) & 0x70) + 0x10;
    }
    return (-(uint)((param_2 & 0x80000000) == 0) & 0xf8) + 8;
  }
  iVar1 = FUN_00419610(param_1,param_2);
  if (iVar1 == 1) {
    return 0x200;
  }
  if (iVar1 != 2) {
    if (iVar1 != 3) {
      return 1;
    }
    return 2;
  }
  return 4;
}



// Library Function - Single Match
//  __raise_exc
// 
// Library: Visual Studio 1998 Release

void __cdecl
__raise_exc(uint *param_1,uint *param_2,uint param_3,int param_4,uint *param_5,uint *param_6)

{
  uint *puVar1;
  uint *puVar2;
  uint uVar3;
  uint *puVar4;
  DWORD local_4;
  
  puVar1 = param_2;
  param_1[1] = 0;
  param_1[2] = 0;
  param_1[3] = 0;
  if ((param_3 & 0x10) != 0) {
    local_4 = 0xc000008f;
    param_1[1] = param_1[1] | 1;
  }
  if ((param_3 & 2) != 0) {
    local_4 = 0xc0000093;
    param_1[1] = param_1[1] | 2;
  }
  if ((param_3 & 1) != 0) {
    local_4 = 0xc0000091;
    param_1[1] = param_1[1] | 4;
  }
  if ((param_3 & 4) != 0) {
    local_4 = 0xc000008e;
    param_1[1] = param_1[1] | 8;
  }
  if ((param_3 & 8) != 0) {
    local_4 = 0xc0000090;
    param_1[1] = param_1[1] | 0x10;
  }
  uVar3 = param_1[2];
  param_1[2] = ((~*param_2 & 1) << 4 ^ uVar3) & 0x10 ^ uVar3;
  uVar3 = param_1[2];
  param_1[2] = ((uint)((*param_2 & 4) == 0) << 3 ^ uVar3) & 8 ^ uVar3;
  uVar3 = param_1[2];
  param_1[2] = ((uint)((*param_2 & 8) == 0) << 2 ^ uVar3) & 4 ^ uVar3;
  uVar3 = param_1[2];
  param_1[2] = ((uint)((*param_2 & 0x10) == 0) * 2 ^ uVar3) & 2 ^ uVar3;
  uVar3 = param_1[2];
  param_1[2] = ((*param_2 & 0x20) == 0 ^ uVar3) & 1 ^ uVar3;
  uVar3 = __statfp();
  puVar2 = param_6;
  if ((uVar3 & 1) != 0) {
    param_1[3] = param_1[3] | 0x10;
  }
  if ((uVar3 & 4) != 0) {
    param_1[3] = param_1[3] | 8;
  }
  if ((uVar3 & 8) != 0) {
    param_1[3] = param_1[3] | 4;
  }
  if ((uVar3 & 0x10) != 0) {
    param_1[3] = param_1[3] | 2;
  }
  if ((uVar3 & 0x20) != 0) {
    param_1[3] = param_1[3] | 1;
  }
  uVar3 = *puVar1 & 0xc00;
  if (uVar3 < 0x401) {
    if (uVar3 == 0x400) {
      *param_1 = *param_1 & 0xfffffffd | 1;
    }
    else if (uVar3 == 0) {
      *param_1 = *param_1 & 0xfffffffc;
    }
  }
  else if (uVar3 == 0x800) {
    *param_1 = *param_1 & 0xfffffffe | 2;
  }
  else if (uVar3 == 0xc00) {
    *param_1 = *param_1 | 3;
  }
  uVar3 = *puVar1 & 0x300;
  if (uVar3 == 0) {
    *param_1 = *param_1 & 0xffffffeb | 8;
  }
  else if (uVar3 == 0x200) {
    *param_1 = *param_1 & 0xffffffe7 | 4;
  }
  else if (uVar3 == 0x300) {
    *param_1 = *param_1 & 0xffffffe3;
  }
  *param_1 = (param_4 << 5 ^ *param_1) & 0x1ffe0 ^ *param_1;
  param_1[8] = param_1[8] | 1;
  param_1[8] = param_1[8] & 0xffffffe3 | 2;
  param_1[5] = param_5[1];
  param_1[4] = *param_5;
  param_1[0x14] = param_1[0x14] | 1;
  param_1[0x14] = param_1[0x14] & 0xffffffe3 | 2;
  param_1[0x11] = param_6[1];
  param_1[0x10] = *param_6;
  __clrfp();
  RaiseException(local_4,0,1,(ULONG_PTR *)&param_1);
  puVar4 = param_1 + 2;
  if ((*puVar4 & 0x10) != 0) {
    *puVar1 = *puVar1 & 0xfffffffe;
  }
  if ((*puVar4 & 8) != 0) {
    *puVar1 = *puVar1 & 0xfffffffb;
  }
  if ((*puVar4 & 4) != 0) {
    *puVar1 = *puVar1 & 0xfffffff7;
  }
  if ((*puVar4 & 2) != 0) {
    *puVar1 = *puVar1 & 0xffffffef;
  }
  if ((*puVar4 & 1) != 0) {
    *puVar1 = *puVar1 & 0xffffffdf;
  }
  switch(*param_1 & 3) {
  case 0:
    *puVar1 = *puVar1 & 0xfffff3ff;
    break;
  case 1:
    *puVar1 = *puVar1 & 0xfffff7ff | 0x400;
    break;
  case 2:
    *puVar1 = *puVar1 & 0xfffffbff | 0x800;
    break;
  case 3:
    *puVar1 = *puVar1 | 0xc00;
  }
  uVar3 = (*param_1 & 0x1c) >> 2;
  if (uVar3 == 0) {
    *puVar1 = *puVar1 & 0xfffff3ff | 0x300;
  }
  else if (uVar3 == 1) {
    *puVar1 = *puVar1 & 0xfffff3ff | 0x200;
  }
  else if (uVar3 == 2) {
    *puVar1 = *puVar1 & 0xfffff3ff;
  }
  puVar2[1] = param_1[0x11];
  *puVar2 = param_1[0x10];
  return;
}



// Library Function - Single Match
//  __handle_exc
// 
// Library: Visual Studio 1998 Release

bool __cdecl __handle_exc(uint param_1,double *param_2,uint param_3)

{
  ulonglong uVar1;
  uint uVar2;
  bool bVar3;
  float10 fVar4;
  undefined8 local_c;
  int local_4;
  
  uVar2 = param_1 & 0x1f;
  if (((param_1 & 8) == 0) || ((param_3 & 1) == 0)) {
    if (((param_1 & 4) == 0) || ((param_3 & 4) == 0)) {
      if (((param_1 & 1) == 0) || ((param_3 & 8) == 0)) {
        if (((param_1 & 2) != 0) && ((param_3 & 0x10) != 0)) {
          bVar3 = (param_1 & 0x10) != 0;
          if (((ulonglong)*param_2 & 0x7fffffff00000000) == 0 && *(int *)param_2 == 0) {
            bVar3 = true;
          }
          else {
            fVar4 = __decomp(*(uint *)param_2,*(uint *)((int)param_2 + 4),&local_4);
            local_4 = local_4 + -0x600;
            if (local_4 < -0x432) {
              bVar3 = true;
              local_c = 0.0;
            }
            else {
              local_c = (double)(ulonglong)
                                (SUB87((double)fVar4,0) & 0xfffffffffffff | 0x10000000000000);
              if (local_4 < -0x3fd) {
                local_4 = -0x3fd - local_4;
                do {
                  if ((((ulonglong)local_c & 1) != 0) && (!bVar3)) {
                    bVar3 = true;
                  }
                  uVar2 = (uint)local_c >> 1;
                  uVar1 = (ulonglong)local_c & 0x100000000;
                  local_c._0_4_ = uVar2;
                  if (uVar1 != 0) {
                    local_c._0_4_ = uVar2 | 0x80000000;
                  }
                  local_c = (double)CONCAT44(local_c._4_4_ >> 1,(uint)local_c);
                  local_4 = local_4 + -1;
                } while (local_4 != 0);
              }
              if ((double)fVar4 < 0.0) {
                local_c = -local_c;
              }
            }
            *(uint *)((int)param_2 + 4) = local_c._4_4_;
            *(uint *)param_2 = (uint)local_c;
          }
          if (bVar3) {
            FUN_00419f30();
          }
          uVar2 = param_1 & 0x1d;
        }
      }
      else {
        FUN_00419f30();
        uVar2 = param_3 & 0xc00;
        if (uVar2 < 0x401) {
          if (uVar2 == 0x400) {
            if (*param_2 <= 0.0) {
              uVar2 = param_1 & 0x1e;
              *param_2 = -(double)CONCAT44(DAT_0043bfbc,DAT_0043bfb8);
            }
            else {
              uVar2 = param_1 & 0x1e;
              *(undefined4 *)((int)param_2 + 4) = DAT_0043bfcc;
              *(undefined4 *)param_2 = DAT_0043bfc8;
            }
          }
          else if (uVar2 == 0) {
            if (*param_2 <= 0.0) {
              uVar2 = param_1 & 0x1e;
              *param_2 = -(double)CONCAT44(DAT_0043bfbc,DAT_0043bfb8);
            }
            else {
              uVar2 = param_1 & 0x1e;
              *(undefined4 *)((int)param_2 + 4) = DAT_0043bfbc;
              *(undefined4 *)param_2 = DAT_0043bfb8;
            }
          }
          else {
            uVar2 = param_1 & 0x1e;
          }
        }
        else if (uVar2 == 0x800) {
          if (*param_2 <= 0.0) {
            uVar2 = param_1 & 0x1e;
            *param_2 = -(double)CONCAT44(DAT_0043bfcc,DAT_0043bfc8);
          }
          else {
            uVar2 = param_1 & 0x1e;
            *(undefined4 *)((int)param_2 + 4) = DAT_0043bfbc;
            *(undefined4 *)param_2 = DAT_0043bfb8;
          }
        }
        else if (uVar2 == 0xc00) {
          if (*param_2 <= 0.0) {
            uVar2 = param_1 & 0x1e;
            *param_2 = -(double)CONCAT44(DAT_0043bfcc,DAT_0043bfc8);
          }
          else {
            uVar2 = param_1 & 0x1e;
            *(undefined4 *)((int)param_2 + 4) = DAT_0043bfcc;
            *(undefined4 *)param_2 = DAT_0043bfc8;
          }
        }
        else {
          uVar2 = param_1 & 0x1e;
        }
      }
    }
    else {
      uVar2 = param_1 & 0x1b;
      FUN_00419f30();
    }
  }
  else {
    uVar2 = param_1 & 0x17;
    FUN_00419f30();
  }
  if (((param_1 & 0x10) != 0) && ((param_3 & 0x20) != 0)) {
    uVar2 = uVar2 & 0xffffffef;
    FUN_00419f30();
  }
  return uVar2 == 0;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __set_errno
// 
// Library: Visual Studio 1998 Release

errno_t __cdecl __set_errno(int _Value)

{
  if (_Value == 1) {
    _DAT_0043aa58 = 0x21;
    return _Value;
  }
  if (1 < _Value) {
    if (3 < _Value) {
      return _Value;
    }
    _DAT_0043aa58 = 0x22;
  }
  return _Value;
}



undefined4 FUN_00419ea0(void)

{
  return 0;
}



// Library Function - Single Match
//  __statfp
// 
// Library: Visual Studio 1998 Release

int __statfp(void)

{
  short in_FPUStatusWord;
  
  return (int)in_FPUStatusWord;
}



// Library Function - Single Match
//  __clrfp
// 
// Library: Visual Studio 1998 Release

int __clrfp(void)

{
  short in_FPUStatusWord;
  
  return (int)in_FPUStatusWord;
}



// Library Function - Single Match
//  __ctrlfp
// 
// Library: Visual Studio 1998 Release

int __ctrlfp(void)

{
  short in_FPUControlWord;
  
  return (int)in_FPUControlWord;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00419f30(void)

{
  return;
}



// Library Function - Single Match
//  ___ld12mul
// 
// Library: Visual Studio 1998 Release

void __cdecl ___ld12mul(int *param_1,int *param_2)

{
  uint *puVar1;
  ushort uVar2;
  int iVar3;
  ushort uVar4;
  ushort uVar5;
  int iVar6;
  short sVar7;
  int iVar8;
  ushort local_1a;
  undefined4 local_18;
  short local_14 [4];
  int local_c;
  int local_8;
  int local_4;
  
  local_18._0_1_ = 0;
  local_18._1_1_ = 0;
  local_18._2_2_ = 0;
  local_14[0] = 0;
  local_14[1] = 0;
  local_14[2] = 0;
  local_14[3] = 0;
  uVar2 = (*(ushort *)((int)param_2 + 10) ^ *(ushort *)((int)param_1 + 10)) & 0x8000;
  uVar4 = *(ushort *)((int)param_1 + 10) & 0x7fff;
  uVar5 = *(ushort *)((int)param_2 + 10) & 0x7fff;
  local_1a = uVar4 + uVar5;
  if (((0x7ffe < uVar4) || (0x7ffe < uVar5)) || (0xbffd < local_1a)) {
    param_1[1] = 0;
    *param_1 = 0;
    param_1[2] = (-(uint)(uVar2 == 0) & 0x80000000) - 0x8000;
    return;
  }
  if (local_1a < 0x3fc0) {
    param_1[2] = 0;
    param_1[1] = 0;
    *param_1 = 0;
    return;
  }
  if (((uVar4 == 0) && (local_1a = local_1a + 1, (param_1[2] & 0x7fffffffU) == 0)) &&
     ((param_1[1] == 0 && (*param_1 == 0)))) {
    *(undefined2 *)((int)param_1 + 10) = 0;
    return;
  }
  if (((uVar5 == 0) && (local_1a = local_1a + 1, (param_2[2] & 0x7fffffffU) == 0)) &&
     ((param_2[1] == 0 && (*param_2 == 0)))) {
    param_1[2] = 0;
    param_1[1] = 0;
    *param_1 = 0;
    return;
  }
  local_8 = 0;
  local_c = 0;
  do {
    iVar6 = 8;
    iVar8 = local_c * 2;
    local_4 = 5 - local_c;
    if (0 < 5 - local_c) {
      puVar1 = (uint *)((int)&local_18 + local_8);
      do {
        iVar3 = ___addl(*puVar1,(uint)*(ushort *)(iVar6 + (int)param_2) *
                                (uint)*(ushort *)(iVar8 + (int)param_1),puVar1);
        if (iVar3 != 0) {
          *(short *)((int)local_14 + local_8) = *(short *)((int)local_14 + local_8) + 1;
        }
        iVar8 = iVar8 + 2;
        iVar6 = iVar6 + -2;
        local_4 = local_4 + -1;
      } while (local_4 != 0);
    }
    local_8 = local_8 + 2;
    local_c = local_c + 1;
  } while (local_c < 5);
  local_1a = local_1a + 0xc002;
  if (0 < (short)local_1a) {
    do {
      if ((local_14[3] & 0x8000U) != 0) break;
      ___shl_12(&local_18);
      local_1a = local_1a - 1;
    } while (0 < (short)local_1a);
    if (0 < (short)local_1a) goto LAB_0041a163;
  }
  local_1a = local_1a - 1;
  if ((short)local_1a < 0) {
    iVar8 = CONCAT22(local_18._2_2_,CONCAT11(local_18._1_1_,(byte)local_18));
    sVar7 = -local_1a;
    local_1a = 0;
    do {
      if (((byte)local_18 & 1) != 0) {
        iVar8 = iVar8 + 1;
      }
      ___shr_12(&local_18);
      sVar7 = sVar7 + -1;
    } while (sVar7 != 0);
  }
  else {
    iVar8 = CONCAT22(local_18._2_2_,CONCAT11(local_18._1_1_,(byte)local_18));
  }
  if (iVar8 != 0) {
    local_18._0_1_ = (byte)local_18 | 1;
  }
LAB_0041a163:
  iVar6 = CONCAT22(local_14[2],local_14[1]);
  iVar8 = CONCAT22(local_14[0],local_18._2_2_);
  if (0x8000 < CONCAT11(local_18._1_1_,(byte)local_18)) {
    if (CONCAT22(local_14[0],local_18._2_2_) == -1) {
      iVar8 = 0;
      if (CONCAT22(local_14[2],local_14[1]) == -1) {
        if (local_14[3] == 0xffff) {
          local_14[3] = 0x8000;
          local_1a = local_1a + 1;
          iVar6 = 0;
          iVar8 = 0;
        }
        else {
          local_14[3] = local_14[3] + 1;
          iVar6 = 0;
          iVar8 = 0;
        }
      }
      else {
        iVar6 = CONCAT22(local_14[2],local_14[1]) + 1;
      }
    }
    else {
      iVar8 = CONCAT22(local_14[0],local_18._2_2_) + 1;
      iVar6 = CONCAT22(local_14[2],local_14[1]);
    }
  }
  local_14[0] = (short)((uint)iVar8 >> 0x10);
  local_18._2_2_ = (undefined2)iVar8;
  local_14[2] = (short)((uint)iVar6 >> 0x10);
  local_14[1] = (short)iVar6;
  if (local_1a < 0x7fff) {
    *(undefined2 *)param_1 = local_18._2_2_;
    *(uint *)((int)param_1 + 2) = CONCAT22(local_14[1],local_14[0]);
    *(ushort *)((int)param_1 + 10) = local_1a | uVar2;
    *(uint *)((int)param_1 + 6) = CONCAT22(local_14[3],local_14[2]);
    return;
  }
  param_1[1] = 0;
  *param_1 = 0;
  param_1[2] = (-(uint)(uVar2 == 0) & 0x80000000) - 0x8000;
  return;
}



// Library Function - Single Match
//  ___multtenpow12
// 
// Library: Visual Studio 1998 Release

void __cdecl ___multtenpow12(int *param_1,uint param_2,int param_3)

{
  ushort *puVar1;
  uint uVar2;
  ushort *puVar3;
  undefined *puVar4;
  uint uVar5;
  ushort local_c;
  undefined4 uStack_a;
  undefined2 uStack_6;
  undefined4 local_4;
  
  puVar4 = &DAT_0043c080;
  if (param_2 != 0) {
    if ((int)param_2 < 0) {
      param_2 = -param_2;
      puVar4 = &DAT_0043c1e0;
    }
    if (param_3 == 0) {
      *(undefined2 *)param_1 = 0;
    }
    while (param_2 != 0) {
      puVar4 = puVar4 + 0x54;
      uVar5 = (int)param_2 >> 3;
      uVar2 = param_2 & 7;
      param_2 = uVar5;
      if (uVar2 != 0) {
        puVar1 = (ushort *)(puVar4 + uVar2 * 0xc);
        puVar3 = puVar1;
        if (0x7fff < *puVar1) {
          puVar3 = &local_c;
          local_c = (ushort)*(undefined4 *)puVar1;
          uStack_a._0_2_ = (undefined2)((uint)*(undefined4 *)puVar1 >> 0x10);
          uStack_a._2_2_ = (undefined2)*(undefined4 *)(puVar1 + 2);
          uStack_6 = (undefined2)((uint)*(undefined4 *)(puVar1 + 2) >> 0x10);
          local_4 = *(undefined4 *)(puVar1 + 4);
          uStack_a = CONCAT22(uStack_a._2_2_,(undefined2)uStack_a) + -1;
        }
        ___ld12mul(param_1,(int *)puVar3);
      }
    }
  }
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address
// Library Function - Single Match
//  __setmode
// 
// Library: Visual Studio 1998 Release

int __cdecl __setmode(int _FileHandle,int _Mode)

{
  byte *pbVar1;
  byte bVar2;
  byte bVar3;
  
  if ((uint)_FileHandle < DAT_00454680) {
    pbVar1 = (byte *)(*(int *)((int)&DAT_00454580 + ((int)(_FileHandle & 0xffffffe7U) >> 3)) + 4 +
                     (_FileHandle & 0x1fU) * 8);
    bVar2 = *pbVar1;
    if ((bVar2 & 1) != 0) {
      if (_Mode == 0x8000) {
        bVar3 = bVar2 & 0x7f;
      }
      else {
        if (_Mode != 0x4000) {
          _DAT_0043aa58 = 0x16;
          return -1;
        }
        bVar3 = bVar2 | 0x80;
      }
      *pbVar1 = bVar3;
      return (-(uint)((bVar2 & 0x80) == 0) & 0x4000) + 0x4000;
    }
  }
  _DAT_0043aa58 = 9;
  return -1;
}



// WARNING: Unable to track spacebase fully for stack

void FUN_0041a340(void)

{
  uint in_EAX;
  undefined1 *puVar1;
  undefined4 unaff_retaddr;
  
  puVar1 = &stack0x00000004;
  if (0xfff < in_EAX) {
    do {
      puVar1 = puVar1 + -0x1000;
      in_EAX = in_EAX - 0x1000;
    } while (0xfff < in_EAX);
  }
  *(undefined4 *)(puVar1 + (-4 - in_EAX)) = unaff_retaddr;
  return;
}



void RtlUnwind(PVOID TargetFrame,PVOID TargetIp,PEXCEPTION_RECORD ExceptionRecord,PVOID ReturnValue)

{
                    // WARNING: Could not recover jumptable at 0x0041a370. Too many branches
                    // WARNING: Treating indirect jump as call
  RtlUnwind(TargetFrame,TargetIp,ExceptionRecord,ReturnValue);
  return;
}



// Library Function - Single Match
//  __strcmpi
// 
// Library: Visual Studio 1998 Release

int __cdecl __strcmpi(char *_Str1,char *_Str2)

{
  char cVar1;
  char cVar2;
  uint uVar3;
  byte bVar4;
  byte bVar5;
  byte bVar6;
  int iVar7;
  int _C;
  
  if (DAT_0043bdd8 == 0) {
    bVar6 = 0xff;
    do {
      do {
        if (bVar6 == 0) goto LAB_0041a3ce;
        bVar6 = *_Str2;
        _Str2 = (char *)((byte *)_Str2 + 1);
        bVar5 = *_Str1;
        _Str1 = (char *)((byte *)_Str1 + 1);
      } while (bVar5 == bVar6);
      bVar4 = bVar6 + 0xbf + (-((byte)(bVar6 + 0xbf) < 0x1a) & 0x20U) + 0x41;
      bVar5 = bVar5 + 0xbf;
      bVar6 = bVar5 + (-(bVar5 < 0x1a) & 0x20U) + 0x41;
    } while (bVar6 == bVar4);
    bVar6 = (bVar6 < bVar4) * -2 + 1;
LAB_0041a3ce:
    iVar7 = (int)(char)bVar6;
  }
  else {
    _C = 0;
    iVar7 = 0xff;
    do {
      do {
        if ((char)iVar7 == '\0') {
          return iVar7;
        }
        cVar1 = *_Str2;
        iVar7 = CONCAT31((int3)((uint)iVar7 >> 8),cVar1);
        _Str2 = _Str2 + 1;
        cVar2 = *_Str1;
        _C = CONCAT31((int3)((uint)_C >> 8),cVar2);
        _Str1 = _Str1 + 1;
      } while (cVar1 == cVar2);
      _C = _tolower(_C);
      iVar7 = _tolower(iVar7);
    } while ((byte)_C == (byte)iVar7);
    uVar3 = (uint)((byte)_C < (byte)iVar7);
    iVar7 = (1 - uVar3) - (uint)(uVar3 != 0);
  }
  return iVar7;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00456000(void)

{
  uint uVar1;
  int iVar2;
  
  DAT_0041cb2c = DAT_0041cab8 - DAT_0041cab0;
  uVar1 = DAT_0041cac0 - DAT_0041cab0;
  DAT_0041cb30 = uVar1;
  if (uVar1 == 0) {
    DAT_0041cb30 = 1;
  }
  if (uVar1 == DAT_0041cb2c) {
    DAT_0041cb48 = 0x7fffffff;
  }
  else {
    DAT_0041cb48 = (uint)(((ulonglong)DAT_0041cb2c << 0x20) / (ulonglong)DAT_0041cb30) >> 1;
  }
  _DAT_0041cb38 = DAT_0041cab4 - DAT_0041caac;
  DAT_0041cb3c = (int)((ulonglong)
                       ((longlong)((DAT_0041cabc - DAT_0041caac) * 2) * (longlong)(int)DAT_0041cb48)
                      >> 0x20);
  DAT_0041cb40 = (int)((ulonglong)
                       ((longlong)((DAT_0041cad4 - DAT_0041cac4) * 2) * (longlong)(int)DAT_0041cb48)
                      >> 0x20);
  DAT_0041cb44 = (int)((ulonglong)
                       ((longlong)((DAT_0041cad8 - DAT_0041cac8) * 2) * (longlong)(int)DAT_0041cb48)
                      >> 0x20);
  DAT_0041cb34 = DAT_0041cb3c - (DAT_0041cab4 - DAT_0041caac);
  if ((-3 < DAT_0041cb34) && (DAT_0041cb34 < 3)) {
    DAT_0041cb34 = (DAT_0041cb34 >> 2 | 1U) << 2;
  }
  iVar2 = (DAT_0041cb40 - DAT_0041cacc) + DAT_0041cac4;
  DAT_0041cadc = (int)(CONCAT44(iVar2 >> 0x10,iVar2 * 0x10000) / (longlong)DAT_0041cb34);
  _DAT_0041caf0 = -DAT_0041cadc >> 3;
  _DAT_0041caec = 0;
  _DAT_0041caf4 = _DAT_0041caf0 * 2;
  _DAT_0041caf8 = _DAT_0041caf0 * 3;
  _DAT_0041cafc = _DAT_0041caf0 * 4;
  _DAT_0041cb00 = _DAT_0041caf0 * 5;
  _DAT_0041cb04 = _DAT_0041caf0 * 6;
  _DAT_0041cb08 = _DAT_0041caf0 * 7;
  iVar2 = (DAT_0041cb44 - DAT_0041cad0) + DAT_0041cac8;
  DAT_0041cae0 = (int)(CONCAT44(iVar2 >> 0x10,iVar2 * 0x10000) / (longlong)DAT_0041cb34);
  _DAT_0041cb10 = -DAT_0041cae0 >> 3;
  _DAT_0041cb0c = 0;
  _DAT_0041cb14 = _DAT_0041cb10 * 2;
  _DAT_0041cb18 = _DAT_0041cb10 * 3;
  _DAT_0041cb1c = _DAT_0041cb10 * 4;
  _DAT_0041cb20 = _DAT_0041cb10 * 5;
  _DAT_0041cb24 = _DAT_0041cb10 * 6;
  _DAT_0041cb28 = _DAT_0041cb10 * 7;
  return;
}



void FUN_00456180(void)

{
  int iVar1;
  int *unaff_EBX;
  int unaff_ESI;
  
  iVar1 = *(int *)(unaff_ESI + 4) >> 8;
  *unaff_EBX = iVar1;
  iVar1 = (*(int *)(unaff_ESI + 0xc) >> 8) - iVar1;
  unaff_EBX[1] = iVar1;
  if (iVar1 != 0) {
    FUN_00456a50();
    FUN_004567a0(unaff_EBX[1],DAT_0041cbf4);
  }
  return;
}



void FUN_004561c0(void)

{
  int iVar1;
  int *unaff_EBX;
  int unaff_ESI;
  
  iVar1 = *(int *)(unaff_ESI + 4) >> 8;
  *unaff_EBX = iVar1;
  iVar1 = (*(int *)(unaff_ESI + 0xc) >> 8) - iVar1;
  unaff_EBX[1] = iVar1;
  if (iVar1 != 0) {
    FUN_00456a50();
    FUN_00456760((short)unaff_EBX[1],DAT_0041cbf4);
  }
  return;
}



void FUN_00456520(void)

{
  int iVar1;
  int *unaff_EBX;
  undefined4 *unaff_ESI;
  
  if (DAT_00453044 < (int)unaff_ESI[3]) {
    if ((int)unaff_ESI[1] < DAT_00453058) {
      FUN_004567f0();
      DAT_0041cbf0 = unaff_EBX + 2;
      iVar1 = unaff_ESI[1];
      *unaff_EBX = iVar1 >> 8;
      iVar1 = ((int)unaff_ESI[3] >> 8) - (iVar1 >> 8);
      unaff_EBX[1] = iVar1;
      if (iVar1 != 0) {
        FUN_00456a50();
        FUN_004567a0(unaff_EBX[1],DAT_0041cbf4);
        DAT_0041cbf0 = unaff_EBX + 2;
      }
      if (DAT_0041cc00 == 1) {
        *unaff_ESI = unaff_ESI[2];
        unaff_ESI[1] = unaff_ESI[3];
        unaff_ESI[4] = unaff_ESI[6];
        unaff_ESI[5] = unaff_ESI[7];
        unaff_ESI[2] = DAT_0041cc04;
        unaff_ESI[3] = DAT_0041cc08;
        unaff_ESI[6] = DAT_0041cc0c;
        unaff_ESI[7] = DAT_0041cc10;
        FUN_00456a09();
        DAT_0041cc14 = ((int)unaff_ESI[3] >> 8) - ((int)unaff_ESI[1] >> 8);
        if (DAT_0041cc14 != 0) {
          unaff_EBX[1] = unaff_EBX[1] + DAT_0041cc14;
          FUN_00456a50();
          FUN_004567a0(DAT_0041cc14,DAT_0041cbf4);
        }
      }
    }
    else {
      unaff_EBX[1] = 0;
      *unaff_EBX = DAT_00453048;
    }
  }
  else {
    unaff_EBX[1] = 0;
    *unaff_EBX = DAT_0045304c;
  }
  return;
}



void FUN_00456640(void)

{
  int iVar1;
  int *unaff_EBX;
  undefined4 *unaff_ESI;
  
  if (DAT_00453044 < (int)unaff_ESI[3]) {
    if ((int)unaff_ESI[1] < DAT_00453058) {
      FUN_004567f0();
      DAT_0041cc1c = unaff_EBX + 2;
      iVar1 = unaff_ESI[1];
      *unaff_EBX = iVar1 >> 8;
      iVar1 = ((int)unaff_ESI[3] >> 8) - (iVar1 >> 8);
      unaff_EBX[1] = iVar1;
      if (iVar1 != 0) {
        FUN_00456a50();
        FUN_00456760((short)unaff_EBX[1],DAT_0041cbf4);
        DAT_0041cc1c = unaff_EBX + 2;
      }
      if (DAT_0041cc00 == 1) {
        *unaff_ESI = unaff_ESI[2];
        unaff_ESI[1] = unaff_ESI[3];
        unaff_ESI[4] = unaff_ESI[6];
        unaff_ESI[5] = unaff_ESI[7];
        unaff_ESI[2] = DAT_0041cc04;
        unaff_ESI[3] = DAT_0041cc08;
        unaff_ESI[6] = DAT_0041cc0c;
        unaff_ESI[7] = DAT_0041cc10;
        FUN_00456a09();
        DAT_0041cc20 = ((int)unaff_ESI[3] >> 8) - ((int)unaff_ESI[1] >> 8);
        if (DAT_0041cc20 != 0) {
          unaff_EBX[1] = unaff_EBX[1] + DAT_0041cc20;
          FUN_00456a50();
          FUN_00456760((short)DAT_0041cc20,DAT_0041cbf4);
        }
      }
    }
    else {
      unaff_EBX[1] = 0;
      *unaff_EBX = DAT_00453048;
    }
  }
  else {
    unaff_EBX[1] = 0;
    *unaff_EBX = DAT_0045304c;
  }
  return;
}



void __fastcall FUN_00456760(short param_1,int param_2)

{
  bool bVar1;
  int iVar2;
  int in_EAX;
  short sVar3;
  int *unaff_EBX;
  int unaff_ESI;
  
  do {
    unaff_EBX[1] = unaff_ESI >> 0x10;
    unaff_ESI = unaff_ESI + DAT_0041cbe4;
    *unaff_EBX = in_EAX >> 0x10;
    iVar2 = DAT_0041cbf8;
    in_EAX = in_EAX + DAT_0041cbe8;
    unaff_EBX[2] = param_2 >> 0x10;
    param_2 = param_2 + iVar2;
    unaff_EBX = unaff_EBX + 3;
    sVar3 = param_1 + -1;
    bVar1 = 0 < param_1;
    param_1 = sVar3;
  } while (sVar3 != 0 && bVar1);
  return;
}



void __fastcall FUN_004567a0(int param_1,uint param_2)

{
  bool bVar1;
  int in_EAX;
  int iVar2;
  int *unaff_EBX;
  int unaff_ESI;
  uint uVar3;
  
  do {
    uVar3 = param_2 >> 0xb & 0x1c;
    unaff_EBX[1] = *(int *)(&DAT_0041caec + uVar3) + unaff_ESI >> 8;
    unaff_ESI = unaff_ESI + DAT_0041cbe4;
    *unaff_EBX = *(int *)(&DAT_0041cb0c + uVar3) + in_EAX >> 8;
    iVar2 = DAT_0041cbf8;
    in_EAX = in_EAX + DAT_0041cbe8;
    unaff_EBX[2] = (int)param_2 >> 0x10;
    param_2 = param_2 + iVar2;
    unaff_EBX = unaff_EBX + 3;
    iVar2 = param_1 + -1;
    bVar1 = 0 < param_1;
    param_1 = iVar2;
  } while (iVar2 != 0 && bVar1);
  return;
}



int FUN_004567f0(void)

{
  int iVar1;
  longlong lVar2;
  uint uVar3;
  int iVar4;
  ushort uVar5;
  uint uVar6;
  int *unaff_ESI;
  
  iVar4 = unaff_ESI[1];
  if (iVar4 < DAT_00453044) {
    uVar3 = (uint)(((ulonglong)(uint)(DAT_00453044 - iVar4) << 0x20) /
                  (ulonglong)(uint)(unaff_ESI[3] - iVar4)) >> 1;
    unaff_ESI[5] = unaff_ESI[5] +
                   (int)((ulonglong)
                         ((longlong)((unaff_ESI[7] - unaff_ESI[5]) * 2) * (longlong)(int)uVar3) >>
                        0x20);
    unaff_ESI[4] = unaff_ESI[4] +
                   (int)((ulonglong)
                         ((longlong)((unaff_ESI[6] - unaff_ESI[4]) * 2) * (longlong)(int)uVar3) >>
                        0x20);
    lVar2 = (longlong)((unaff_ESI[2] - *unaff_ESI) * 2) * (longlong)(int)uVar3;
    iVar4 = (int)lVar2;
    *unaff_ESI = *unaff_ESI + (int)((ulonglong)lVar2 >> 0x20);
    unaff_ESI[1] = DAT_00453044;
  }
  iVar1 = unaff_ESI[3];
  if (DAT_00453058 < iVar1) {
    uVar3 = (uint)(((ulonglong)(uint)(iVar1 - DAT_00453058) << 0x20) /
                  (ulonglong)(uint)(iVar1 - unaff_ESI[1])) >> 1;
    unaff_ESI[7] = unaff_ESI[7] -
                   (int)((ulonglong)
                         ((longlong)((unaff_ESI[7] - unaff_ESI[5]) * 2) * (longlong)(int)uVar3) >>
                        0x20);
    unaff_ESI[6] = unaff_ESI[6] -
                   (int)((ulonglong)
                         ((longlong)((unaff_ESI[6] - unaff_ESI[4]) * 2) * (longlong)(int)uVar3) >>
                        0x20);
    lVar2 = (longlong)((unaff_ESI[2] - *unaff_ESI) * 2) * (longlong)(int)uVar3;
    iVar4 = (int)lVar2;
    unaff_ESI[2] = unaff_ESI[2] - (int)((ulonglong)lVar2 >> 0x20);
    unaff_ESI[3] = DAT_00453058;
  }
  DAT_0041cc00 = 0;
  if (DAT_00453064 < *unaff_ESI) {
    if (unaff_ESI[2] < DAT_00453064) {
      DAT_0041cc00 = 1;
      DAT_0041cc04 = unaff_ESI[2];
      DAT_0041cc08 = unaff_ESI[3];
      DAT_0041cc0c = unaff_ESI[6];
      DAT_0041cc10 = unaff_ESI[7];
      uVar3 = (uint)(((ulonglong)(uint)(DAT_00453064 - unaff_ESI[2]) << 0x20) /
                    (ulonglong)(uint)(*unaff_ESI - unaff_ESI[2])) >> 1;
      unaff_ESI[6] = unaff_ESI[6] -
                     (int)((ulonglong)
                           ((longlong)((unaff_ESI[6] - unaff_ESI[4]) * 2) * (longlong)(int)uVar3) >>
                          0x20);
      unaff_ESI[7] = unaff_ESI[7] -
                     (int)((ulonglong)
                           ((longlong)((unaff_ESI[7] - unaff_ESI[5]) * 2) * (longlong)(int)uVar3) >>
                          0x20);
      unaff_ESI[3] = unaff_ESI[3] -
                     (int)((ulonglong)
                           ((longlong)((unaff_ESI[3] - unaff_ESI[1]) * 2) * (longlong)(int)uVar3) >>
                          0x20);
      unaff_ESI[2] = DAT_00453064;
    }
    else {
      iVar4 = unaff_ESI[2] - DAT_00453064;
      lVar2 = (longlong)iVar4 * (longlong)DAT_0041cadc;
      uVar5 = (ushort)((ulonglong)lVar2 >> 0x10);
      unaff_ESI[6] = unaff_ESI[6] -
                     ((uint)uVar5 | CONCAT22(uVar5,(short)((ulonglong)lVar2 >> 0x20)) << 0x10);
      lVar2 = (longlong)iVar4 * (longlong)DAT_0041cae0;
      uVar5 = (ushort)((ulonglong)lVar2 >> 0x10);
      unaff_ESI[7] = unaff_ESI[7] -
                     ((uint)uVar5 | CONCAT22(uVar5,(short)((ulonglong)lVar2 >> 0x20)) << 0x10);
      unaff_ESI[2] = DAT_00453064;
    }
    iVar4 = *unaff_ESI - DAT_00453064;
    lVar2 = (longlong)iVar4 * (longlong)DAT_0041cadc;
    uVar5 = (ushort)((ulonglong)lVar2 >> 0x10);
    unaff_ESI[4] = unaff_ESI[4] -
                   ((uint)uVar5 | CONCAT22(uVar5,(short)((ulonglong)lVar2 >> 0x20)) << 0x10);
    lVar2 = (longlong)iVar4 * (longlong)DAT_0041cae0;
    uVar5 = (ushort)((ulonglong)lVar2 >> 0x10);
    unaff_ESI[5] = unaff_ESI[5] -
                   ((uint)uVar5 | CONCAT22(uVar5,(short)((ulonglong)lVar2 >> 0x20)) << 0x10);
    iVar4 = DAT_00453064;
    *unaff_ESI = DAT_00453064;
  }
  if (DAT_00453064 < unaff_ESI[2]) {
    DAT_0041cc00 = 1;
    DAT_0041cc04 = unaff_ESI[2];
    DAT_0041cc08 = unaff_ESI[3];
    DAT_0041cc0c = unaff_ESI[6];
    DAT_0041cc10 = unaff_ESI[7];
    uVar6 = unaff_ESI[2] - DAT_00453064;
    uVar3 = unaff_ESI[2] - *unaff_ESI;
    if (uVar6 == uVar3) {
      uVar3 = 0xffffffff;
    }
    else {
      uVar3 = (uint)(((ulonglong)uVar6 << 0x20) / (ulonglong)uVar3);
    }
    uVar3 = uVar3 >> 1;
    unaff_ESI[6] = unaff_ESI[6] -
                   (int)((ulonglong)
                         ((longlong)((unaff_ESI[6] - unaff_ESI[4]) * 2) * (longlong)(int)uVar3) >>
                        0x20);
    unaff_ESI[7] = unaff_ESI[7] -
                   (int)((ulonglong)
                         ((longlong)((unaff_ESI[7] - unaff_ESI[5]) * 2) * (longlong)(int)uVar3) >>
                        0x20);
    unaff_ESI[3] = unaff_ESI[3] -
                   (int)((ulonglong)
                         ((longlong)((unaff_ESI[3] - unaff_ESI[1]) * 2) * (longlong)(int)uVar3) >>
                        0x20);
    iVar4 = DAT_00453064;
    unaff_ESI[2] = DAT_00453064;
  }
  return iVar4;
}



void FUN_00456a09(void)

{
  longlong lVar1;
  ushort uVar2;
  int iVar3;
  int unaff_ESI;
  
  if (DAT_00453064 < *(int *)(unaff_ESI + 8)) {
    iVar3 = *(int *)(unaff_ESI + 8) - DAT_00453064;
    lVar1 = (longlong)iVar3 * (longlong)DAT_0041cadc;
    uVar2 = (ushort)((ulonglong)lVar1 >> 0x10);
    *(int *)(unaff_ESI + 0x18) =
         *(int *)(unaff_ESI + 0x18) -
         ((uint)uVar2 | CONCAT22(uVar2,(short)((ulonglong)lVar1 >> 0x20)) << 0x10);
    lVar1 = (longlong)iVar3 * (longlong)DAT_0041cae0;
    uVar2 = (ushort)((ulonglong)lVar1 >> 0x10);
    *(int *)(unaff_ESI + 0x1c) =
         *(int *)(unaff_ESI + 0x1c) -
         ((uint)uVar2 | CONCAT22(uVar2,(short)((ulonglong)lVar1 >> 0x20)) << 0x10);
    *(int *)(unaff_ESI + 8) = DAT_00453064;
  }
  return;
}



void FUN_00456a50(void)

{
  uint uVar1;
  uint uVar2;
  int iVar3;
  int *unaff_ESI;
  
  uVar2 = unaff_ESI[3] - unaff_ESI[1];
  if (uVar2 < 0x41) {
    uVar1 = unaff_ESI[2] - *unaff_ESI;
    if ((int)uVar1 < 1) {
      uVar1 = -uVar1;
    }
    if (uVar1 < 0x10001) {
      uVar2 = 0x41;
    }
    else if (uVar2 < 4) {
      uVar2 = 3;
    }
  }
  DAT_0041cbe8 = (int)(CONCAT44(unaff_ESI[7] - unaff_ESI[5] >> 0x1f,
                                (unaff_ESI[7] - unaff_ESI[5]) * 0x10000) / (longlong)(int)uVar2);
  DAT_0041cbe4 = (int)(CONCAT44(unaff_ESI[6] - unaff_ESI[4] >> 0x1f,
                                (unaff_ESI[6] - unaff_ESI[4]) * 0x10000) / (longlong)(int)uVar2);
  DAT_0041cbf8 = (int)(CONCAT44(unaff_ESI[2] - *unaff_ESI >> 0x10,
                                (unaff_ESI[2] - *unaff_ESI) * 0x10000) / (longlong)(int)uVar2);
  iVar3 = 0x100 - (unaff_ESI[1] & 0xffU);
  DAT_0041cbdc = (iVar3 * DAT_0041cbe4 >> 8) + unaff_ESI[4] * 0x100;
  DAT_0041cbe0 = (iVar3 * DAT_0041cbe8 >> 8) + unaff_ESI[5] * 0x100;
  DAT_0041cbf4 = iVar3 * (DAT_0041cbf8 >> 8) + *unaff_ESI * 0x100;
  return;
}



