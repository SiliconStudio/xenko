// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#include <windows.h>
#include <map>
#include <string>
#include <DbgHelp.h>
#pragma comment(lib, "DbgHelp.lib")

class FnHook
{
public:

 typedef struct FnEntry
 {
 HANDLE pModule;
 PIMAGE_IMPORT_DESCRIPTOR pImageDsc;
 PIMAGE_THUNK_DATA pThunkDat;
 UINT uThunkIdx;
 UINT uFnHint;
 PVOID ppFnOld;
 PVOID ppFnNew;
 PVOID ppFnAdr;
 }FnEntry;

 std::map<std::string,FnEntry> vFn;



 void HookFunction( CHAR *pSrc, CHAR *pDst, UINT uHint, PVOID pFnNew )
 {
 PSTR pName;
 ULONG ulSize;
 FnEntry fn;
 memset(&fn,0,sizeof(fn));

 fn.ppFnNew = &pFnNew;
 fn.uFnHint = uHint;
 fn.pModule = GetModuleHandleA(pSrc);

 fn.pImageDsc = (PIMAGE_IMPORT_DESCRIPTOR)ImageDirectoryEntryToData( fn.pModule, TRUE, IMAGE_DIRECTORY_ENTRY_IMPORT, &ulSize );
 while( fn.pImageDsc->Name )
 {
  pName = (PSTR) ( (PBYTE)fn.pModule + fn.pImageDsc->Name );
  if( lstrcmpiA(pDst, pName) == 0 )
  goto Hook0;
  fn.pImageDsc++;
 }
 return;

 Hook0:

 fn.pThunkDat = (PIMAGE_THUNK_DATA) ( (PBYTE)fn.pModule + fn.pImageDsc->OriginalFirstThunk );
 while( fn.pThunkDat )
 {
 IMAGE_IMPORT_BY_NAME* pImp = (IMAGE_IMPORT_BY_NAME*)((PBYTE)fn.pModule+fn.pThunkDat->u1.AddressOfData);
 if( pImp->Hint == uHint )
 {
 fn.pThunkDat = (PIMAGE_THUNK_DATA) ( (PBYTE)fn.pModule + fn.pImageDsc->FirstThunk );
 fn.ppFnAdr = &((fn.pThunkDat+fn.uThunkIdx)->u1.Function);
 goto Hook1;
 }
 fn.uThunkIdx++;
 fn.pThunkDat++;
 }
 return;

 Hook1:

 fn.ppFnOld = WriteFn( fn.ppFnAdr, fn.ppFnNew );

 CHAR Hint[5];
 sprintf_s(Hint, "%4.4X", uHint);
 std::string key = std::string(pSrc)+"|"
 + std::string(pDst)+"|"
 + std::string(Hint);

 auto fnIt = vFn.find(key);
 if( fnIt == vFn.end() )
 {
 vFn.insert( std::make_pair(key, fn) );
 }
 }

 void UnHookFunction( CHAR *pSrc, CHAR *pDst, UINT uHint )
 {
 CHAR Hint[5];
 sprintf_s(Hint, "%4.4X", uHint);
 std::string key = std::string(pSrc)+"|"
 + std::string(pDst)+"|"
 + std::string(Hint);

 auto fnIt = vFn.find(key);
 if( fnIt != vFn.end() )
 {
 WriteFn( fnIt->second.ppFnAdr, fnIt->second.ppFnOld );
 vFn.erase( fnIt );
 }

 return;
 }

 void UnHookAll()
 {
 for( auto fnIt = vFn.begin(); fnIt != vFn.end(); fnIt++ )
 {
 WriteFn( fnIt->second.ppFnAdr, fnIt->second.ppFnOld );
 }
 vFn.clear();
 }

 PVOID WriteFn( PVOID ppFnAdr, PVOID ppFnNew )
 {
 BOOL bProtectResult = FALSE;
 DWORD dwOldProtect = 0;
 SIZE_T sBytes;
 PVOID ppFnOld=NULL;

 MEMORY_BASIC_INFORMATION memInfo;
 if( VirtualQuery(ppFnAdr, &memInfo, sizeof( memInfo ) ) > 0 )
 {
 bProtectResult = VirtualProtect( memInfo.BaseAddress, memInfo.RegionSize, PAGE_READWRITE, &dwOldProtect );

 ReadProcessMemory( GetCurrentProcess(), ppFnAdr, &ppFnOld, sizeof(PROC*), &sBytes );
 WriteProcessMemory( GetCurrentProcess(), ppFnAdr, ppFnNew, sizeof(PROC*), &sBytes );

 // restore the page to its old protect status
 bProtectResult = VirtualProtect( memInfo.BaseAddress, memInfo.RegionSize, PAGE_READONLY, &dwOldProtect );
 }

 return ppFnOld;
 }

};