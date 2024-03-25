using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AllenNeuralDynamics.Cuttlefish
{
    [Description("Class for helper methods.")]
    public class HelperMethods
    {
        [Description("Converts a struct to its byte array representation.")]
        internal static unsafe byte[] StructToByteArray(PwmTaskPayload value)
        {
            var payload = new[] { value };
            byte[] bytes = new byte[payload.Length * sizeof(PwmTaskPayload)];

            fixed (void* src = payload)
            {
                fixed (void* dst = bytes)
                {
                    // Buffer.BlockCopy(payload, 0, bytes, 0, bytes.Length);
                    Buffer.MemoryCopy(src, dst, bytes.Length, bytes.Length);
                }
            }
            return bytes;
        }


        internal static unsafe PwmTaskPayload ByteArrayToStruct(byte[] value)
        {
            PwmTaskPayload[] newArray = new PwmTaskPayload[1];
            Buffer.BlockCopy(value, 0, newArray, 0, value.Length);
            return newArray[0];
        }

        [Description("Struct of settings for the PWM task.")]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct PwmTaskPayload
        {
            public uint delay;
            public uint onTime;
            public uint period;
            public byte portMask;
            public uint repeats;
            public byte invert;
        }

    }
}