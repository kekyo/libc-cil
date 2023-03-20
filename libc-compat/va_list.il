.assembly 'libc-compat'
{
    .custom instance void [mscorlib]System.Runtime.CompilerServices.InternalsVisibleToAttribute::.ctor(string) = (
        01 00 04 6c 69 62 63 00 00
    )
    .custom instance void [mscorlib]System.Runtime.CompilerServices.InternalsVisibleToAttribute::.ctor(string) = (
        01 00 0e 6c 69 62 63 2d 62 6f 6f 74 73 74 72 61
        70 00 00
    )

    .hash algorithm 0x00008004
    .ver 1:0:0:0
}

.namespace C.type
{
    .class public auto ansi abstract beforefieldinit va_list
        extends [mscorlib]System.Object
    {
        .method assembly hidebysig specialname rtspecialname 
            instance void .ctor() cil managed 
        {
            ldarg.0
            call instance void [mscorlib]System.Object::.ctor()
            ret
        }

        .method assembly hidebysig newslot strict abstract virtual 
            instance typedref __get() cil managed 
        {
        }
    }

    .class private auto ansi sealed beforefieldinit __va_list_impl
        extends C.type.va_list
    {
        .field private initonly valuetype [mscorlib]System.ArgIterator iterator

        .method public hidebysig specialname rtspecialname 
            instance void .ctor(valuetype [mscorlib]System.RuntimeArgumentHandle handle) cil managed
        {
            ldarg.0
            dup
            call instance void C.type.va_list::.ctor()
            ldarg.1
            newobj instance void [mscorlib]System.ArgIterator::.ctor(valuetype [mscorlib]System.RuntimeArgumentHandle)
            stfld valuetype [mscorlib]System.ArgIterator C.type.__va_list_impl::iterator
            ret
        }

        .method assembly hidebysig strict virtual
            instance typedref __get() cil managed
        {
            ldarg.0
            ldflda valuetype [mscorlib]System.ArgIterator C.type.__va_list_impl::iterator
            call instance typedref [mscorlib]System.ArgIterator::GetNextArg()
            ret
        }
    }

    .class private auto ansi sealed beforefieldinit __va_list_runtime_impl
        extends C.type.va_list
    {
        .field private static initonly object[] empty
        .field private static initonly class [mscorlib]System.Type 'type'
        .field private static initonly class [mscorlib]System.Reflection.MethodInfo getNextArgMethod

        .method private hidebysig specialname rtspecialname static 
            void .cctor() cil managed 
        {
            .locals init (
                [0] class [mscorlib]System.Type[],
                [1] int32,
                [2] class [mscorlib]System.Type t,
                [3] bool
            )

            ldc.i4.0
            newarr [mscorlib]System.Object
            stsfld object[] C.type.__va_list_runtime_impl::empty
            ldtoken [mscorlib]System.TypedReference
            call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
            callvirt instance class [mscorlib]System.Reflection.Assembly [mscorlib]System.Type::get_Assembly()
            callvirt instance class [mscorlib]System.Type[] [mscorlib]System.Reflection.Assembly::GetTypes()
            stloc.0
            br.s IL_004d
        IL_0026:
            ldloc.0
            ldloc.1
            ldelem.ref
            stloc.2
            ldloc.2
            callvirt instance string [mscorlib]System.Type::get_FullName()
            ldstr "System.ArgIterator"
            call bool [mscorlib]System.String::op_Equality(string, string)
            stloc.3
            ldloc.3
            brfalse.s IL_0048
            ldloc.2
            stsfld class [mscorlib]System.Type C.type.__va_list_runtime_impl::'type'
            br.s IL_0053
        IL_0048:
            ldloc.1
            ldc.i4.1
            add
            stloc.1
        IL_004d:
            ldloc.1
            ldloc.0
            ldlen
            conv.i4
            blt.s IL_0026
        IL_0053:
            ldsfld class [mscorlib]System.Type C.type.__va_list_runtime_impl::'type'
            ldstr "GetNextArg"
            ldsfld class [mscorlib]System.Type[] [mscorlib]System.Type::EmptyTypes
            callvirt instance class [mscorlib]System.Reflection.MethodInfo [mscorlib]System.Type::GetMethod(string, class [mscorlib]System.Type[])
            stsfld class [mscorlib]System.Reflection.MethodInfo C.type.__va_list_runtime_impl::getNextArgMethod
            ret
        }

        .field private initonly object iterator

        .method public hidebysig specialname rtspecialname 
            instance void .ctor (
                valuetype [mscorlib]System.RuntimeArgumentHandle handle) cil managed 
        {
            ldarg.0
            call instance void C.type.va_list::.ctor()

            ldarg.0
            ldsfld class [mscorlib]System.Type C.type.__va_list_runtime_impl::'type'
            ldc.i4.1
            newarr [mscorlib]System.Object
            dup
            ldc.i4.0
            ldarg.1
            box [mscorlib]System.RuntimeArgumentHandle
            stelem.ref
            call object [mscorlib]System.Activator::CreateInstance(class [mscorlib]System.Type, object[])
            stfld object C.type.__va_list_runtime_impl::iterator
            ret
        }

        .method assembly hidebysig strict virtual 
            instance typedref __get() cil managed 
        {
            ldsfld class [mscorlib]System.Reflection.MethodInfo C.type.__va_list_runtime_impl::getNextArgMethod
            ldarg.0
            ldfld object C.type.__va_list_runtime_impl::iterator
            ldsfld object[] C.type.__va_list_runtime_impl::empty
            callvirt instance object [mscorlib]System.Reflection.MethodBase::Invoke(object, object[])
            unbox.any typedref
            ret
        }
    }
}
