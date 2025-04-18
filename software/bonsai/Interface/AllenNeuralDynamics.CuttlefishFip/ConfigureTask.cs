using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reactive.Linq;
using Bonsai.Harp;
using Bonsai;
using System.Collections.Generic;

namespace AllenNeuralDynamics.CuttlefishFip
{
    /// <summary>
    /// Represents an operator that generates a sequence of Harp messages to
    /// configure a task.
    /// </summary>
    [Description("Generates a sequence of Harp messages to configure a task.")]
    public class ConfigureTask : Source<HarpMessage>
    {

        private Type register = typeof(AddTask);

        private int address
        {
            get
            {
                var registerValue = RegisterFromAddress(register);
                if (registerValue.HasValue)
                {
                    return registerValue.Value;
                }
                else
                {
                    throw new ArgumentException($"The register {register.Name} is not a valid register.");
                }
            }
        }
        /// <summary>
        /// Gets or sets the type of the Harp Message
        /// </summary>
        [Description("The type of the Harp message.")]
        public MessageType MessageType { get; set; } = MessageType.Write;

        private Ports pwmPort = Ports.IO0;
        /// <summary>
        /// Gets or sets the port for the PWM Task.
        /// </summary>
        [Description("The PWM port. Only a single flag can be high.")]
        public Ports PwmPort
        {
            get => pwmPort;
            set
            {
                if ((value & (value - 1)) != 0)
                {
                    throw new ArgumentException("Only a single PWM port flag can be set.");
                }
                pwmPort = value;
            }
        }

        /// <summary>
        /// Gets or sets the duty cycle (0-1) of the PWM port in the task.
        /// </summary>
        [Description("The duty cycle of the PWM.")]
        [Range(0.0, 1.0)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        public float DutyCycle { get; set; } = 0;

        /// <summary>
        /// Gets or sets the frequency (Hz) of the PWM port in the task.
        /// </summary>
        [Description("The frequency of the PWM.")]
        [Range(5000, 100000)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        public float Frequency { get; set; } = 10000;


        /// <summary>
        /// Gets or sets the port to be used by the trigger.
        /// </summary>
        [Description("The port of the trigger.")]
        public Ports TriggerPorts { get; set; } = Ports.IO1;


        /// <summary>
        /// Gets or sets the state of the events
        /// </summary>
        [Description("The state of the events.")]
        public bool EventsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the state of the mute task
        /// </summary>
        [Description("Mutes the output of the task while preserving the timing.")]
        public bool IsMuted { get; set; } = false;

        /// <summary>
        /// Gets or sets duration of Delta1 delay (in microseconds).
        /// </summary>
        [Description("The length of the Delta1 delay.")]
        public uint Delta1 { get; set; } = 0;

        /// <summary>
        /// Gets or sets duration of Delta2 delay (in microseconds).
        /// </summary>
        [Description("The length of the Delta2 delay.")]
        public uint Delta2 { get; set; } = 0;


        /// <summary>
        /// Gets or sets duration of Delta3 delay (in microseconds).
        /// </summary>
        [Description("The length of the Delta3 delay.")]
        public uint Delta3 { get; set; } = 0;

        /// <summary>
        /// Gets or sets duration of Delta4 delay (in microseconds).
        /// </summary>
        [Description("The length of the Delta4 delay.")]
        public uint Delta4 { get; set; } = 0;



        /// <summary>
        /// Generates an observable sequence of Harp messages to configure a
        /// task.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="HarpMessage"/> objects representing a command
        /// to configure a Task.
        /// </returns>
        public override IObservable<HarpMessage> Generate()
        {
            return Observable.Return(BuildMessage(address, MessageType, null));
        }

        /// <summary>
        /// Generates an observable sequence of Harp messages to configure the
        /// Task feature whenever the source sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to emit new configuration
        /// messages.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="HarpMessage"/> objects representing the commands
        /// needed to fully configure a task.
        /// </returns>
        public IObservable<HarpMessage> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => BuildMessage(address, MessageType, null));
        }

        private HarpMessage BuildMessage(int address, MessageType messageType, double? timestamp = null)
        {

            var payload = new TaskPayload()
            {
                pwmPort = (uint)PwmPort,
                dutyCycle = DutyCycle,
                frequency = Frequency,
                triggerPorts = (uint)TriggerPorts,
                eventsEnabled = EventsEnabled ? (byte)1 : (byte)0,
                isMuted = IsMuted ? (byte)1 : (byte)0,
                delta1 = Delta1,
                delta2 = Delta2,
                delta3 = Delta3,
                delta4 = Delta4
            };
            var bytes = HelperMethods.StructToByteArray(payload);
            if (timestamp.HasValue)
            {
                return HarpMessage.FromPayload(address, timestamp.Value, messageType, PayloadType.U8, bytes);
            }
            else
            {
                return HarpMessage.FromPayload(address, messageType, PayloadType.U8, bytes);
            }
        }

        private static int? RegisterFromAddress(Type value)
        {
            foreach (var kv in Device.RegisterMap)
            {
                if (kv.Value == value)
                {
                    return kv.Key;
                }
            }
            return null;
        }
    }


}
