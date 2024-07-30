using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai.Harp;
using Bonsai;

namespace AllenNeuralDynamics.Cuttlefish
{
    /// <summary>
    /// Represents an operator that generates a sequence of Harp messages to
    /// configure the PWM feature.
    /// </summary>
    [Description("Generates a sequence of Harp messages to configure the PWM feature.")]
    public class ConfigurePwm : Source<HarpMessage>
    {

        /// <summary>
        /// Gets or sets the address of the Harp Message
        /// </summary>
        [Description("The address of the register to be used to configure the PWM task.")]
        public int Address { get; set; } = (int) PwmTask.Address;

        /// <summary>
        /// Gets or sets the type of the Harp Message
        /// </summary>
        [Description("The type of the Harp message.")]
        public MessageType MessageType { get; set; } = MessageType.Write;

        /// <summary>
        /// Gets or sets the PWM protocol delay.
        /// </summary>
        [Description("The delay to start the PWM protocol after the trigger is activated.")]
        public uint Delay { get; set; } = 0;

        /// <summary>
        /// Gets or sets the on-time of the PWM pulse. Defined in microseconds.
        /// </summary>
        [Description("The time the pulse spends on the High state. Defined in microseconds.")]
        public uint OnTime { get; set; } = 500000;

        /// <summary>
        /// Gets or sets the period of the PWM pulse. Defined in microseconds.
        /// </summary>
        [Description("The period of the PWM pulse.")]
        public uint Period { get; set; } = 1000000;

        /// <summary>
        /// Gets or sets the number of pulses to trigger on the specified PWM.
        /// If the default value of zero is specified, the PWM will be infinite.
        /// </summary>
        [Description("The number of pulses to trigger on the specified PWM. If the default value of zero is specified, the PWM will be infinite.")]
        public Ports Port { get; set; } = 0x0;

        /// <summary>
        /// Gets or sets the number of times the PWM protocol will be repeated.
        /// </summary>
        [Description("The number of time the PWM protocol will be repeated.")]
        public uint RepeatCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value specifying whether generation of the PWM should be inverted.
        /// </summary>
        [Description("Specifies whether the pulse should be inverted.")]
        public bool Invert { get; set; } = false;


        /// <summary>
        /// Generates an observable sequence of Harp messages to configure a
        /// PWM task.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="HarpMessage"/> objects representing a command
        /// to configure a PWM task.
        /// </returns>
        public override IObservable<HarpMessage> Generate()
        {
            return Observable.Return(BuildMessage(Address, MessageType, null));
        }

        /// <summary>
        /// Generates an observable sequence of Harp messages to configure the
        /// PWM feature whenever the source sequence emits a notification.
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
        /// needed to fully configure the PWM feature.
        /// </returns>
        public IObservable<HarpMessage> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => BuildMessage(Address, MessageType, null));
        }

        /// <summary>
        /// Builds a message to configure the PWM task.
        /// </summary>
        public HarpMessage BuildMessage(int address, MessageType messageType, double? timestamp = null)
        {
            var payload = new HelperMethods.PwmTaskPayload()
            {
                delay = Delay,
                onTime = OnTime,
                period = Period,
                portMask = (byte)Port,
                repeats = RepeatCount,
                invert = Invert ? (byte)1 : (byte)0
            };
            var bytes = HelperMethods.StructToByteArray(payload);
            if (timestamp.HasValue)
            {
                return HarpMessage.FromPayload(address, timestamp.Value, messageType, PayloadType.U8, bytes);
            }
            else {
                return HarpMessage.FromPayload(address, messageType, PayloadType.U8, bytes);
            }
        }
    }
}