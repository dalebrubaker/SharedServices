using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Scope = "member",
        Target = "BruSoftware.SharedServices.ZipCodec.#Compress(System.Byte[],System.IO.Compression.CompressionLevel,System.Int32,System.Int32)")]
[assembly:
    SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Scope = "member",
        Target = "BruSoftware.SharedServices.ZipCodec.#CompressToFile(System.Byte[],System.String,System.IO.Compression.CompressionLevel)")]
[assembly:
    SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Scope = "member",
        Target = "BruSoftware.SharedServices.ZipCodec.#Decompress(System.Byte[])")]
[assembly:
    SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Scope = "member",
        Target = "BruSoftware.SharedServices.ZipCodec.#DecompressFromFile(System.String)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member",
        Target =
            "BruSoftware.SharedServices.Config.WindowPlacement.#GetWindowPlacement(System.IntPtr,BruSoftware.SharedServices.Config.WINDOWPLACEMENT&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member",
        Target =
            "BruSoftware.SharedServices.Config.WindowPlacement.#SetWindowPlacement(System.IntPtr,BruSoftware.SharedServices.Config.WINDOWPLACEMENT&)")]
[assembly:
    SuppressMessage("Design", "CC0120:Your Switch maybe include default clause", Justification = "<Pending>", Scope = "member",
        Target = "~M:BruSoftware.SharedServices.BitArrayFast.#ctor(System.Byte[])")]
[assembly:
    SuppressMessage("Naming", "VSSpell001:Spell Check", Justification = "<Pending>", Scope = "type",
        Target = "~T:BruSoftware.SharedServices.Exceptions.BruTraderException")]

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Code Analysis results, point to "Suppress Message", and click
// "In Suppression File".
// You do not need to add suppressions to this file manually.