using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AllenNeuralDynamics.CuttlefishFip
{
    [Description("Class for helper methods.")]
    public class HelperMethods
    {
        [Description("Converts a struct to its byte array representation.")]
        internal static unsafe byte[] StructToByteArray(TaskPayload value)
        {
            var payload = new[] { value };
            byte[] bytes = new byte[payload.Length * sizeof(TaskPayload)];

            fixed (void* src = payload)
            {
                fixed (void* dst = bytes)
                {
                    Buffer.MemoryCopy(src, dst, bytes.Length, bytes.Length);
                }
            }
            return bytes;
        }

        internal static unsafe TaskPayload ByteArrayToStruct(byte[] value)
        {
            TaskPayload[] newArray = new TaskPayload[1];
            Buffer.BlockCopy(value, 0, newArray, 0, value.Length);
            return newArray[0];
        }

    }
    [Description("Struct of settings for the task.")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaskPayload
    {
        public uint pwmPort;
        public float dutyCycle;
        public float frequency;
        public uint triggerPorts;
        public byte eventsEnabled;
        public byte isMuted;
        public uint delta1;
        public uint delta2;
        public uint delta3;
        public uint delta4;
    }
}
