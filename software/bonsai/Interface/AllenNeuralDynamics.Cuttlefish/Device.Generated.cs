using Bonsai;
using Bonsai.Harp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace AllenNeuralDynamics.Cuttlefish
{
    /// <summary>
    /// Generates events and processes commands for the Cuttlefish device connected
    /// at the specified serial port.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Generates events and processes commands for the Cuttlefish device.")]
    public partial class Device : Bonsai.Harp.Device, INamedElement
    {
        /// <summary>
        /// Represents the unique identity class of the <see cref="Cuttlefish"/> device.
        /// This field is constant.
        /// </summary>
        public const int WhoAmI = 1403;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device() : base(WhoAmI) { }

        string INamedElement.Name => nameof(Cuttlefish);

        /// <summary>
        /// Gets a read-only mapping from address to register type.
        /// </summary>
        public static new IReadOnlyDictionary<int, Type> RegisterMap { get; } = new Dictionary<int, Type>
            (Bonsai.Harp.Device.RegisterMap.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            { 32, typeof(PortDirection) },
            { 33, typeof(PortState) },
            { 34, typeof(PwmTask) },
            { 35, typeof(ArmExternalStartTrigger) },
            { 36, typeof(ExternalStartTriggerEdge) },
            { 37, typeof(ArmExternalStopTrigger) },
            { 38, typeof(ExternalStopTriggerEdge) },
            { 39, typeof(SoftwareStartTrigger) },
            { 40, typeof(SoftwareStopTrigger) },
            { 41, typeof(TaskControl) }
        };

        /// <summary>
        /// Gets the contents of the metadata file describing the <see cref="Cuttlefish"/>
        /// device registers.
        /// </summary>
        public static readonly string Metadata = GetDeviceMetadata();

        static string GetDeviceMetadata()
        {
            var deviceType = typeof(Device);
            using var metadataStream = deviceType.Assembly.GetManifestResourceStream($"{deviceType.Namespace}.device.yml");
            using var streamReader = new System.IO.StreamReader(metadataStream);
            return streamReader.ReadToEnd();
        }
    }

    /// <summary>
    /// Represents an operator that returns the contents of the metadata file
    /// describing the <see cref="Cuttlefish"/> device registers.
    /// </summary>
    [Description("Returns the contents of the metadata file describing the Cuttlefish device registers.")]
    public partial class GetMetadata : Source<string>
    {
        /// <summary>
        /// Returns an observable sequence with the contents of the metadata file
        /// describing the <see cref="Cuttlefish"/> device registers.
        /// </summary>
        /// <returns>
        /// A sequence with a single <see cref="string"/> object representing the
        /// contents of the metadata file.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Return(Device.Metadata);
        }
    }

    /// <summary>
    /// Represents an operator that groups the sequence of <see cref="Cuttlefish"/>" messages by register type.
    /// </summary>
    [Description("Groups the sequence of Cuttlefish messages by register type.")]
    public partial class GroupByRegister : Combinator<HarpMessage, IGroupedObservable<Type, HarpMessage>>
    {
        /// <summary>
        /// Groups an observable sequence of <see cref="Cuttlefish"/> messages
        /// by register type.
        /// </summary>
        /// <param name="source">The sequence of Harp device messages.</param>
        /// <returns>
        /// A sequence of observable groups, each of which corresponds to a unique
        /// <see cref="Cuttlefish"/> register.
        /// </returns>
        public override IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<HarpMessage> source)
        {
            return source.GroupBy(message => Device.RegisterMap[message.Address]);
        }
    }

    /// <summary>
    /// Represents an operator that filters register-specific messages
    /// reported by the <see cref="Cuttlefish"/> device.
    /// </summary>
    /// <seealso cref="PortDirection"/>
    /// <seealso cref="PortState"/>
    /// <seealso cref="PwmTask"/>
    /// <seealso cref="ArmExternalStartTrigger"/>
    /// <seealso cref="ExternalStartTriggerEdge"/>
    /// <seealso cref="ArmExternalStopTrigger"/>
    /// <seealso cref="ExternalStopTriggerEdge"/>
    /// <seealso cref="SoftwareStartTrigger"/>
    /// <seealso cref="SoftwareStopTrigger"/>
    /// <seealso cref="TaskControl"/>
    [XmlInclude(typeof(PortDirection))]
    [XmlInclude(typeof(PortState))]
    [XmlInclude(typeof(PwmTask))]
    [XmlInclude(typeof(ArmExternalStartTrigger))]
    [XmlInclude(typeof(ExternalStartTriggerEdge))]
    [XmlInclude(typeof(ArmExternalStopTrigger))]
    [XmlInclude(typeof(ExternalStopTriggerEdge))]
    [XmlInclude(typeof(SoftwareStartTrigger))]
    [XmlInclude(typeof(SoftwareStopTrigger))]
    [XmlInclude(typeof(TaskControl))]
    [Description("Filters register-specific messages reported by the Cuttlefish device.")]
    public class FilterRegister : FilterRegisterBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterRegister"/> class.
        /// </summary>
        public FilterRegister()
        {
            Register = new PortDirection();
        }

        string INamedElement.Name
        {
            get => $"{nameof(Cuttlefish)}.{GetElementDisplayName(Register)}";
        }
    }

    /// <summary>
    /// Represents an operator which filters and selects specific messages
    /// reported by the Cuttlefish device.
    /// </summary>
    /// <seealso cref="PortDirection"/>
    /// <seealso cref="PortState"/>
    /// <seealso cref="PwmTask"/>
    /// <seealso cref="ArmExternalStartTrigger"/>
    /// <seealso cref="ExternalStartTriggerEdge"/>
    /// <seealso cref="ArmExternalStopTrigger"/>
    /// <seealso cref="ExternalStopTriggerEdge"/>
    /// <seealso cref="SoftwareStartTrigger"/>
    /// <seealso cref="SoftwareStopTrigger"/>
    /// <seealso cref="TaskControl"/>
    [XmlInclude(typeof(PortDirection))]
    [XmlInclude(typeof(PortState))]
    [XmlInclude(typeof(PwmTask))]
    [XmlInclude(typeof(ArmExternalStartTrigger))]
    [XmlInclude(typeof(ExternalStartTriggerEdge))]
    [XmlInclude(typeof(ArmExternalStopTrigger))]
    [XmlInclude(typeof(ExternalStopTriggerEdge))]
    [XmlInclude(typeof(SoftwareStartTrigger))]
    [XmlInclude(typeof(SoftwareStopTrigger))]
    [XmlInclude(typeof(TaskControl))]
    [XmlInclude(typeof(TimestampedPortDirection))]
    [XmlInclude(typeof(TimestampedPortState))]
    [XmlInclude(typeof(TimestampedPwmTask))]
    [XmlInclude(typeof(TimestampedArmExternalStartTrigger))]
    [XmlInclude(typeof(TimestampedExternalStartTriggerEdge))]
    [XmlInclude(typeof(TimestampedArmExternalStopTrigger))]
    [XmlInclude(typeof(TimestampedExternalStopTriggerEdge))]
    [XmlInclude(typeof(TimestampedSoftwareStartTrigger))]
    [XmlInclude(typeof(TimestampedSoftwareStopTrigger))]
    [XmlInclude(typeof(TimestampedTaskControl))]
    [Description("Filters and selects specific messages reported by the Cuttlefish device.")]
    public partial class Parse : ParseBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parse"/> class.
        /// </summary>
        public Parse()
        {
            Register = new PortDirection();
        }

        string INamedElement.Name => $"{nameof(Cuttlefish)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents an operator which formats a sequence of values as specific
    /// Cuttlefish register messages.
    /// </summary>
    /// <seealso cref="PortDirection"/>
    /// <seealso cref="PortState"/>
    /// <seealso cref="PwmTask"/>
    /// <seealso cref="ArmExternalStartTrigger"/>
    /// <seealso cref="ExternalStartTriggerEdge"/>
    /// <seealso cref="ArmExternalStopTrigger"/>
    /// <seealso cref="ExternalStopTriggerEdge"/>
    /// <seealso cref="SoftwareStartTrigger"/>
    /// <seealso cref="SoftwareStopTrigger"/>
    /// <seealso cref="TaskControl"/>
    [XmlInclude(typeof(PortDirection))]
    [XmlInclude(typeof(PortState))]
    [XmlInclude(typeof(PwmTask))]
    [XmlInclude(typeof(ArmExternalStartTrigger))]
    [XmlInclude(typeof(ExternalStartTriggerEdge))]
    [XmlInclude(typeof(ArmExternalStopTrigger))]
    [XmlInclude(typeof(ExternalStopTriggerEdge))]
    [XmlInclude(typeof(SoftwareStartTrigger))]
    [XmlInclude(typeof(SoftwareStopTrigger))]
    [XmlInclude(typeof(TaskControl))]
    [Description("Formats a sequence of values as specific Cuttlefish register messages.")]
    public partial class Format : FormatBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Format"/> class.
        /// </summary>
        public Format()
        {
            Register = new PortDirection();
        }

        string INamedElement.Name => $"{nameof(Cuttlefish)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents a register that set the direction of the ports.
    /// </summary>
    [Description("Set the direction of the ports")]
    public partial class PortDirection
    {
        /// <summary>
        /// Represents the address of the <see cref="PortDirection"/> register. This field is constant.
        /// </summary>
        public const int Address = 32;

        /// <summary>
        /// Represents the payload type of the <see cref="PortDirection"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="PortDirection"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="PortDirection"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Ports GetPayload(HarpMessage message)
        {
            return (Ports)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="PortDirection"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Ports)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="PortDirection"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PortDirection"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="PortDirection"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PortDirection"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// PortDirection register.
    /// </summary>
    /// <seealso cref="PortDirection"/>
    [Description("Filters and selects timestamped messages from the PortDirection register.")]
    public partial class TimestampedPortDirection
    {
        /// <summary>
        /// Represents the address of the <see cref="PortDirection"/> register. This field is constant.
        /// </summary>
        public const int Address = PortDirection.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="PortDirection"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetPayload(HarpMessage message)
        {
            return PortDirection.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that read or write the state of the ports. An event will be triggered when the state changes without a write command.
    /// </summary>
    [Description("Read or write the state of the ports. An event will be triggered when the state changes without a write command.")]
    public partial class PortState
    {
        /// <summary>
        /// Represents the address of the <see cref="PortState"/> register. This field is constant.
        /// </summary>
        public const int Address = 33;

        /// <summary>
        /// Represents the payload type of the <see cref="PortState"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="PortState"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="PortState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Ports GetPayload(HarpMessage message)
        {
            return (Ports)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="PortState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Ports)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="PortState"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PortState"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="PortState"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PortState"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// PortState register.
    /// </summary>
    /// <seealso cref="PortState"/>
    [Description("Filters and selects timestamped messages from the PortState register.")]
    public partial class TimestampedPortState
    {
        /// <summary>
        /// Represents the address of the <see cref="PortState"/> register. This field is constant.
        /// </summary>
        public const int Address = PortState.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="PortState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetPayload(HarpMessage message)
        {
            return PortState.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).
    /// </summary>
    [Description("Struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8)")]
    public partial class PwmTask
    {
        /// <summary>
        /// Represents the address of the <see cref="PwmTask"/> register. This field is constant.
        /// </summary>
        public const int Address = 34;

        /// <summary>
        /// Represents the payload type of the <see cref="PwmTask"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="PwmTask"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 18;

        /// <summary>
        /// Returns the payload data for <see cref="PwmTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="PwmTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="PwmTask"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PwmTask"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="PwmTask"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PwmTask"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// PwmTask register.
    /// </summary>
    /// <seealso cref="PwmTask"/>
    [Description("Filters and selects timestamped messages from the PwmTask register.")]
    public partial class TimestampedPwmTask
    {
        /// <summary>
        /// Represents the address of the <see cref="PwmTask"/> register. This field is constant.
        /// </summary>
        public const int Address = PwmTask.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="PwmTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return PwmTask.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that if set to 1, the device will execute the PMW task using the selected pins.
    /// </summary>
    [Description("If set to 1, the device will execute the PMW task using the selected pins.")]
    public partial class ArmExternalStartTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="ArmExternalStartTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = 35;

        /// <summary>
        /// Represents the payload type of the <see cref="ArmExternalStartTrigger"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="ArmExternalStartTrigger"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="ArmExternalStartTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Ports GetPayload(HarpMessage message)
        {
            return (Ports)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="ArmExternalStartTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Ports)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="ArmExternalStartTrigger"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ArmExternalStartTrigger"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="ArmExternalStartTrigger"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ArmExternalStartTrigger"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// ArmExternalStartTrigger register.
    /// </summary>
    /// <seealso cref="ArmExternalStartTrigger"/>
    [Description("Filters and selects timestamped messages from the ArmExternalStartTrigger register.")]
    public partial class TimestampedArmExternalStartTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="ArmExternalStartTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = ArmExternalStartTrigger.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="ArmExternalStartTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetPayload(HarpMessage message)
        {
            return ArmExternalStartTrigger.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that set the edge of the external trigger. 0: Rising, 1: Falling.
    /// </summary>
    [Description("Set the edge of the external trigger. 0: Rising, 1: Falling")]
    public partial class ExternalStartTriggerEdge
    {
        /// <summary>
        /// Represents the address of the <see cref="ExternalStartTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const int Address = 36;

        /// <summary>
        /// Represents the payload type of the <see cref="ExternalStartTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="ExternalStartTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="ExternalStartTriggerEdge"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Ports GetPayload(HarpMessage message)
        {
            return (Ports)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="ExternalStartTriggerEdge"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Ports)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="ExternalStartTriggerEdge"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ExternalStartTriggerEdge"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="ExternalStartTriggerEdge"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ExternalStartTriggerEdge"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// ExternalStartTriggerEdge register.
    /// </summary>
    /// <seealso cref="ExternalStartTriggerEdge"/>
    [Description("Filters and selects timestamped messages from the ExternalStartTriggerEdge register.")]
    public partial class TimestampedExternalStartTriggerEdge
    {
        /// <summary>
        /// Represents the address of the <see cref="ExternalStartTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const int Address = ExternalStartTriggerEdge.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="ExternalStartTriggerEdge"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetPayload(HarpMessage message)
        {
            return ExternalStartTriggerEdge.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that if set to 1, the device will stop the PMW task using the selected pins.
    /// </summary>
    [Description("If set to 1, the device will stop the PMW task using the selected pins.")]
    public partial class ArmExternalStopTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="ArmExternalStopTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = 37;

        /// <summary>
        /// Represents the payload type of the <see cref="ArmExternalStopTrigger"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="ArmExternalStopTrigger"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="ArmExternalStopTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Ports GetPayload(HarpMessage message)
        {
            return (Ports)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="ArmExternalStopTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Ports)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="ArmExternalStopTrigger"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ArmExternalStopTrigger"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="ArmExternalStopTrigger"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ArmExternalStopTrigger"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// ArmExternalStopTrigger register.
    /// </summary>
    /// <seealso cref="ArmExternalStopTrigger"/>
    [Description("Filters and selects timestamped messages from the ArmExternalStopTrigger register.")]
    public partial class TimestampedArmExternalStopTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="ArmExternalStopTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = ArmExternalStopTrigger.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="ArmExternalStopTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetPayload(HarpMessage message)
        {
            return ArmExternalStopTrigger.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that set the edge of the external trigger. 0: Rising, 1: Falling.
    /// </summary>
    [Description("Set the edge of the external trigger. 0: Rising, 1: Falling")]
    public partial class ExternalStopTriggerEdge
    {
        /// <summary>
        /// Represents the address of the <see cref="ExternalStopTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const int Address = 38;

        /// <summary>
        /// Represents the payload type of the <see cref="ExternalStopTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="ExternalStopTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="ExternalStopTriggerEdge"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Ports GetPayload(HarpMessage message)
        {
            return (Ports)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="ExternalStopTriggerEdge"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Ports)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="ExternalStopTriggerEdge"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ExternalStopTriggerEdge"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="ExternalStopTriggerEdge"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ExternalStopTriggerEdge"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// ExternalStopTriggerEdge register.
    /// </summary>
    /// <seealso cref="ExternalStopTriggerEdge"/>
    [Description("Filters and selects timestamped messages from the ExternalStopTriggerEdge register.")]
    public partial class TimestampedExternalStopTriggerEdge
    {
        /// <summary>
        /// Represents the address of the <see cref="ExternalStopTriggerEdge"/> register. This field is constant.
        /// </summary>
        public const int Address = ExternalStopTriggerEdge.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="ExternalStopTriggerEdge"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetPayload(HarpMessage message)
        {
            return ExternalStopTriggerEdge.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that writing a non-0 value to this register will trigger the PWM task.
    /// </summary>
    [Description("Writing a non-0 value to this register will trigger the PWM task.")]
    public partial class SoftwareStartTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="SoftwareStartTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = 39;

        /// <summary>
        /// Represents the payload type of the <see cref="SoftwareStartTrigger"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="SoftwareStartTrigger"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="SoftwareStartTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="SoftwareStartTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="SoftwareStartTrigger"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SoftwareStartTrigger"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="SoftwareStartTrigger"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SoftwareStartTrigger"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// SoftwareStartTrigger register.
    /// </summary>
    /// <seealso cref="SoftwareStartTrigger"/>
    [Description("Filters and selects timestamped messages from the SoftwareStartTrigger register.")]
    public partial class TimestampedSoftwareStartTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="SoftwareStartTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = SoftwareStartTrigger.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="SoftwareStartTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return SoftwareStartTrigger.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that writing a non-0 value to this register will stop the PWM task.
    /// </summary>
    [Description("Writing a non-0 value to this register will stop the PWM task.")]
    public partial class SoftwareStopTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="SoftwareStopTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = 40;

        /// <summary>
        /// Represents the payload type of the <see cref="SoftwareStopTrigger"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="SoftwareStopTrigger"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="SoftwareStopTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="SoftwareStopTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="SoftwareStopTrigger"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SoftwareStopTrigger"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="SoftwareStopTrigger"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SoftwareStopTrigger"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// SoftwareStopTrigger register.
    /// </summary>
    /// <seealso cref="SoftwareStopTrigger"/>
    [Description("Filters and selects timestamped messages from the SoftwareStopTrigger register.")]
    public partial class TimestampedSoftwareStopTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="SoftwareStopTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = SoftwareStopTrigger.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="SoftwareStopTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return SoftwareStopTrigger.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that manipulates messages from register TaskControl.
    /// </summary>
    [Description("")]
    public partial class TaskControl
    {
        /// <summary>
        /// Represents the address of the <see cref="TaskControl"/> register. This field is constant.
        /// </summary>
        public const int Address = 41;

        /// <summary>
        /// Represents the payload type of the <see cref="TaskControl"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="TaskControl"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        static TaskControlPayload ParsePayload(byte payload)
        {
            TaskControlPayload result;
            result.ClearAllTasks = (EnableFlag)(byte)(payload & 0x1);
            result.DumpAllTasks = (EnableFlag)(byte)((payload & 0x2) >> 1);
            result.TaskCount = (byte)((payload & 0xF0) >> 4);
            return result;
        }

        static byte FormatPayload(TaskControlPayload value)
        {
            byte result;
            result = (byte)((byte)value.ClearAllTasks & 0x1);
            result |= (byte)(((byte)value.DumpAllTasks << 1) & 0x2);
            result |= (byte)((value.TaskCount << 4) & 0xF0);
            return result;
        }

        /// <summary>
        /// Returns the payload data for <see cref="TaskControl"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static TaskControlPayload GetPayload(HarpMessage message)
        {
            return ParsePayload(message.GetPayloadByte());
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="TaskControl"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<TaskControlPayload> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create(ParsePayload(payload.Value), payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="TaskControl"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TaskControl"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, TaskControlPayload value)
        {
            return HarpMessage.FromByte(Address, messageType, FormatPayload(value));
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="TaskControl"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TaskControl"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, TaskControlPayload value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, FormatPayload(value));
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// TaskControl register.
    /// </summary>
    /// <seealso cref="TaskControl"/>
    [Description("Filters and selects timestamped messages from the TaskControl register.")]
    public partial class TimestampedTaskControl
    {
        /// <summary>
        /// Represents the address of the <see cref="TaskControl"/> register. This field is constant.
        /// </summary>
        public const int Address = TaskControl.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="TaskControl"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<TaskControlPayload> GetPayload(HarpMessage message)
        {
            return TaskControl.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents an operator which creates standard message payloads for the
    /// Cuttlefish device.
    /// </summary>
    /// <seealso cref="CreatePortDirectionPayload"/>
    /// <seealso cref="CreatePortStatePayload"/>
    /// <seealso cref="CreatePwmTaskPayload"/>
    /// <seealso cref="CreateArmExternalStartTriggerPayload"/>
    /// <seealso cref="CreateExternalStartTriggerEdgePayload"/>
    /// <seealso cref="CreateArmExternalStopTriggerPayload"/>
    /// <seealso cref="CreateExternalStopTriggerEdgePayload"/>
    /// <seealso cref="CreateSoftwareStartTriggerPayload"/>
    /// <seealso cref="CreateSoftwareStopTriggerPayload"/>
    /// <seealso cref="CreateTaskControlPayload"/>
    [XmlInclude(typeof(CreatePortDirectionPayload))]
    [XmlInclude(typeof(CreatePortStatePayload))]
    [XmlInclude(typeof(CreatePwmTaskPayload))]
    [XmlInclude(typeof(CreateArmExternalStartTriggerPayload))]
    [XmlInclude(typeof(CreateExternalStartTriggerEdgePayload))]
    [XmlInclude(typeof(CreateArmExternalStopTriggerPayload))]
    [XmlInclude(typeof(CreateExternalStopTriggerEdgePayload))]
    [XmlInclude(typeof(CreateSoftwareStartTriggerPayload))]
    [XmlInclude(typeof(CreateSoftwareStopTriggerPayload))]
    [XmlInclude(typeof(CreateTaskControlPayload))]
    [XmlInclude(typeof(CreateTimestampedPortDirectionPayload))]
    [XmlInclude(typeof(CreateTimestampedPortStatePayload))]
    [XmlInclude(typeof(CreateTimestampedPwmTaskPayload))]
    [XmlInclude(typeof(CreateTimestampedArmExternalStartTriggerPayload))]
    [XmlInclude(typeof(CreateTimestampedExternalStartTriggerEdgePayload))]
    [XmlInclude(typeof(CreateTimestampedArmExternalStopTriggerPayload))]
    [XmlInclude(typeof(CreateTimestampedExternalStopTriggerEdgePayload))]
    [XmlInclude(typeof(CreateTimestampedSoftwareStartTriggerPayload))]
    [XmlInclude(typeof(CreateTimestampedSoftwareStopTriggerPayload))]
    [XmlInclude(typeof(CreateTimestampedTaskControlPayload))]
    [Description("Creates standard message payloads for the Cuttlefish device.")]
    public partial class CreateMessage : CreateMessageBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMessage"/> class.
        /// </summary>
        public CreateMessage()
        {
            Payload = new CreatePortDirectionPayload();
        }

        string INamedElement.Name => $"{nameof(Cuttlefish)}.{GetElementDisplayName(Payload)}";
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that set the direction of the ports.
    /// </summary>
    [DisplayName("PortDirectionPayload")]
    [Description("Creates a message payload that set the direction of the ports.")]
    public partial class CreatePortDirectionPayload
    {
        /// <summary>
        /// Gets or sets the value that set the direction of the ports.
        /// </summary>
        [Description("The value that set the direction of the ports.")]
        public Ports PortDirection { get; set; }

        /// <summary>
        /// Creates a message payload for the PortDirection register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Ports GetPayload()
        {
            return PortDirection;
        }

        /// <summary>
        /// Creates a message that set the direction of the ports.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the PortDirection register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.PortDirection.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that set the direction of the ports.
    /// </summary>
    [DisplayName("TimestampedPortDirectionPayload")]
    [Description("Creates a timestamped message payload that set the direction of the ports.")]
    public partial class CreateTimestampedPortDirectionPayload : CreatePortDirectionPayload
    {
        /// <summary>
        /// Creates a timestamped message that set the direction of the ports.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the PortDirection register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.PortDirection.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that read or write the state of the ports. An event will be triggered when the state changes without a write command.
    /// </summary>
    [DisplayName("PortStatePayload")]
    [Description("Creates a message payload that read or write the state of the ports. An event will be triggered when the state changes without a write command.")]
    public partial class CreatePortStatePayload
    {
        /// <summary>
        /// Gets or sets the value that read or write the state of the ports. An event will be triggered when the state changes without a write command.
        /// </summary>
        [Description("The value that read or write the state of the ports. An event will be triggered when the state changes without a write command.")]
        public Ports PortState { get; set; }

        /// <summary>
        /// Creates a message payload for the PortState register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Ports GetPayload()
        {
            return PortState;
        }

        /// <summary>
        /// Creates a message that read or write the state of the ports. An event will be triggered when the state changes without a write command.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the PortState register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.PortState.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that read or write the state of the ports. An event will be triggered when the state changes without a write command.
    /// </summary>
    [DisplayName("TimestampedPortStatePayload")]
    [Description("Creates a timestamped message payload that read or write the state of the ports. An event will be triggered when the state changes without a write command.")]
    public partial class CreateTimestampedPortStatePayload : CreatePortStatePayload
    {
        /// <summary>
        /// Creates a timestamped message that read or write the state of the ports. An event will be triggered when the state changes without a write command.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the PortState register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.PortState.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).
    /// </summary>
    [DisplayName("PwmTaskPayload")]
    [Description("Creates a message payload that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).")]
    public partial class CreatePwmTaskPayload
    {
        /// <summary>
        /// Gets or sets the value that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).
        /// </summary>
        [Description("The value that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).")]
        public byte[] PwmTask { get; set; }

        /// <summary>
        /// Creates a message payload for the PwmTask register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return PwmTask;
        }

        /// <summary>
        /// Creates a message that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the PwmTask register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.PwmTask.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).
    /// </summary>
    [DisplayName("TimestampedPwmTaskPayload")]
    [Description("Creates a timestamped message payload that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).")]
    public partial class CreateTimestampedPwmTaskPayload : CreatePwmTaskPayload
    {
        /// <summary>
        /// Creates a timestamped message that struct to configure the PWM task. offset_us (U32), start_time_us (U32), stop_time_us (U32), port_mask (U8), cycles (U32),invert (U8).
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the PwmTask register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.PwmTask.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that if set to 1, the device will execute the PMW task using the selected pins.
    /// </summary>
    [DisplayName("ArmExternalStartTriggerPayload")]
    [Description("Creates a message payload that if set to 1, the device will execute the PMW task using the selected pins.")]
    public partial class CreateArmExternalStartTriggerPayload
    {
        /// <summary>
        /// Gets or sets the value that if set to 1, the device will execute the PMW task using the selected pins.
        /// </summary>
        [Description("The value that if set to 1, the device will execute the PMW task using the selected pins.")]
        public Ports ArmExternalStartTrigger { get; set; }

        /// <summary>
        /// Creates a message payload for the ArmExternalStartTrigger register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Ports GetPayload()
        {
            return ArmExternalStartTrigger;
        }

        /// <summary>
        /// Creates a message that if set to 1, the device will execute the PMW task using the selected pins.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the ArmExternalStartTrigger register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ArmExternalStartTrigger.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that if set to 1, the device will execute the PMW task using the selected pins.
    /// </summary>
    [DisplayName("TimestampedArmExternalStartTriggerPayload")]
    [Description("Creates a timestamped message payload that if set to 1, the device will execute the PMW task using the selected pins.")]
    public partial class CreateTimestampedArmExternalStartTriggerPayload : CreateArmExternalStartTriggerPayload
    {
        /// <summary>
        /// Creates a timestamped message that if set to 1, the device will execute the PMW task using the selected pins.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the ArmExternalStartTrigger register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ArmExternalStartTrigger.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that set the edge of the external trigger. 0: Rising, 1: Falling.
    /// </summary>
    [DisplayName("ExternalStartTriggerEdgePayload")]
    [Description("Creates a message payload that set the edge of the external trigger. 0: Rising, 1: Falling.")]
    public partial class CreateExternalStartTriggerEdgePayload
    {
        /// <summary>
        /// Gets or sets the value that set the edge of the external trigger. 0: Rising, 1: Falling.
        /// </summary>
        [Description("The value that set the edge of the external trigger. 0: Rising, 1: Falling.")]
        public Ports ExternalStartTriggerEdge { get; set; }

        /// <summary>
        /// Creates a message payload for the ExternalStartTriggerEdge register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Ports GetPayload()
        {
            return ExternalStartTriggerEdge;
        }

        /// <summary>
        /// Creates a message that set the edge of the external trigger. 0: Rising, 1: Falling.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the ExternalStartTriggerEdge register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ExternalStartTriggerEdge.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that set the edge of the external trigger. 0: Rising, 1: Falling.
    /// </summary>
    [DisplayName("TimestampedExternalStartTriggerEdgePayload")]
    [Description("Creates a timestamped message payload that set the edge of the external trigger. 0: Rising, 1: Falling.")]
    public partial class CreateTimestampedExternalStartTriggerEdgePayload : CreateExternalStartTriggerEdgePayload
    {
        /// <summary>
        /// Creates a timestamped message that set the edge of the external trigger. 0: Rising, 1: Falling.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the ExternalStartTriggerEdge register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ExternalStartTriggerEdge.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that if set to 1, the device will stop the PMW task using the selected pins.
    /// </summary>
    [DisplayName("ArmExternalStopTriggerPayload")]
    [Description("Creates a message payload that if set to 1, the device will stop the PMW task using the selected pins.")]
    public partial class CreateArmExternalStopTriggerPayload
    {
        /// <summary>
        /// Gets or sets the value that if set to 1, the device will stop the PMW task using the selected pins.
        /// </summary>
        [Description("The value that if set to 1, the device will stop the PMW task using the selected pins.")]
        public Ports ArmExternalStopTrigger { get; set; }

        /// <summary>
        /// Creates a message payload for the ArmExternalStopTrigger register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Ports GetPayload()
        {
            return ArmExternalStopTrigger;
        }

        /// <summary>
        /// Creates a message that if set to 1, the device will stop the PMW task using the selected pins.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the ArmExternalStopTrigger register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ArmExternalStopTrigger.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that if set to 1, the device will stop the PMW task using the selected pins.
    /// </summary>
    [DisplayName("TimestampedArmExternalStopTriggerPayload")]
    [Description("Creates a timestamped message payload that if set to 1, the device will stop the PMW task using the selected pins.")]
    public partial class CreateTimestampedArmExternalStopTriggerPayload : CreateArmExternalStopTriggerPayload
    {
        /// <summary>
        /// Creates a timestamped message that if set to 1, the device will stop the PMW task using the selected pins.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the ArmExternalStopTrigger register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ArmExternalStopTrigger.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that set the edge of the external trigger. 0: Rising, 1: Falling.
    /// </summary>
    [DisplayName("ExternalStopTriggerEdgePayload")]
    [Description("Creates a message payload that set the edge of the external trigger. 0: Rising, 1: Falling.")]
    public partial class CreateExternalStopTriggerEdgePayload
    {
        /// <summary>
        /// Gets or sets the value that set the edge of the external trigger. 0: Rising, 1: Falling.
        /// </summary>
        [Description("The value that set the edge of the external trigger. 0: Rising, 1: Falling.")]
        public Ports ExternalStopTriggerEdge { get; set; }

        /// <summary>
        /// Creates a message payload for the ExternalStopTriggerEdge register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Ports GetPayload()
        {
            return ExternalStopTriggerEdge;
        }

        /// <summary>
        /// Creates a message that set the edge of the external trigger. 0: Rising, 1: Falling.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the ExternalStopTriggerEdge register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ExternalStopTriggerEdge.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that set the edge of the external trigger. 0: Rising, 1: Falling.
    /// </summary>
    [DisplayName("TimestampedExternalStopTriggerEdgePayload")]
    [Description("Creates a timestamped message payload that set the edge of the external trigger. 0: Rising, 1: Falling.")]
    public partial class CreateTimestampedExternalStopTriggerEdgePayload : CreateExternalStopTriggerEdgePayload
    {
        /// <summary>
        /// Creates a timestamped message that set the edge of the external trigger. 0: Rising, 1: Falling.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the ExternalStopTriggerEdge register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.ExternalStopTriggerEdge.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that writing a non-0 value to this register will trigger the PWM task.
    /// </summary>
    [DisplayName("SoftwareStartTriggerPayload")]
    [Description("Creates a message payload that writing a non-0 value to this register will trigger the PWM task.")]
    public partial class CreateSoftwareStartTriggerPayload
    {
        /// <summary>
        /// Gets or sets the value that writing a non-0 value to this register will trigger the PWM task.
        /// </summary>
        [Description("The value that writing a non-0 value to this register will trigger the PWM task.")]
        public byte SoftwareStartTrigger { get; set; }

        /// <summary>
        /// Creates a message payload for the SoftwareStartTrigger register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return SoftwareStartTrigger;
        }

        /// <summary>
        /// Creates a message that writing a non-0 value to this register will trigger the PWM task.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the SoftwareStartTrigger register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.SoftwareStartTrigger.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that writing a non-0 value to this register will trigger the PWM task.
    /// </summary>
    [DisplayName("TimestampedSoftwareStartTriggerPayload")]
    [Description("Creates a timestamped message payload that writing a non-0 value to this register will trigger the PWM task.")]
    public partial class CreateTimestampedSoftwareStartTriggerPayload : CreateSoftwareStartTriggerPayload
    {
        /// <summary>
        /// Creates a timestamped message that writing a non-0 value to this register will trigger the PWM task.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the SoftwareStartTrigger register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.SoftwareStartTrigger.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that writing a non-0 value to this register will stop the PWM task.
    /// </summary>
    [DisplayName("SoftwareStopTriggerPayload")]
    [Description("Creates a message payload that writing a non-0 value to this register will stop the PWM task.")]
    public partial class CreateSoftwareStopTriggerPayload
    {
        /// <summary>
        /// Gets or sets the value that writing a non-0 value to this register will stop the PWM task.
        /// </summary>
        [Description("The value that writing a non-0 value to this register will stop the PWM task.")]
        public byte SoftwareStopTrigger { get; set; }

        /// <summary>
        /// Creates a message payload for the SoftwareStopTrigger register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return SoftwareStopTrigger;
        }

        /// <summary>
        /// Creates a message that writing a non-0 value to this register will stop the PWM task.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the SoftwareStopTrigger register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.SoftwareStopTrigger.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that writing a non-0 value to this register will stop the PWM task.
    /// </summary>
    [DisplayName("TimestampedSoftwareStopTriggerPayload")]
    [Description("Creates a timestamped message payload that writing a non-0 value to this register will stop the PWM task.")]
    public partial class CreateTimestampedSoftwareStopTriggerPayload : CreateSoftwareStopTriggerPayload
    {
        /// <summary>
        /// Creates a timestamped message that writing a non-0 value to this register will stop the PWM task.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the SoftwareStopTrigger register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.SoftwareStopTrigger.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// for register TaskControl.
    /// </summary>
    [DisplayName("TaskControlPayload")]
    [Description("Creates a message payload for register TaskControl.")]
    public partial class CreateTaskControlPayload
    {
        /// <summary>
        /// Gets or sets a value that halts and clears all tasks.
        /// </summary>
        [Description("Halts and clears all tasks.")]
        public EnableFlag ClearAllTasks { get; set; }

        /// <summary>
        /// Gets or sets a value that sends an event from PwmTask register per currently configured task. Once all events have been sent, a write message will be returned from this register.
        /// </summary>
        [Description("Sends an event from PwmTask register per currently configured task. Once all events have been sent, a write message will be returned from this register.")]
        public EnableFlag DumpAllTasks { get; set; }

        /// <summary>
        /// Gets or sets a value that number of tasks currently configured. This portiion of the register is read-only.
        /// </summary>
        [Description("Number of tasks currently configured. This portiion of the register is read-only.")]
        public byte TaskCount { get; set; }

        /// <summary>
        /// Creates a message payload for the TaskControl register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public TaskControlPayload GetPayload()
        {
            TaskControlPayload value;
            value.ClearAllTasks = ClearAllTasks;
            value.DumpAllTasks = DumpAllTasks;
            value.TaskCount = TaskCount;
            return value;
        }

        /// <summary>
        /// Creates a message for register TaskControl.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the TaskControl register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.TaskControl.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// for register TaskControl.
    /// </summary>
    [DisplayName("TimestampedTaskControlPayload")]
    [Description("Creates a timestamped message payload for register TaskControl.")]
    public partial class CreateTimestampedTaskControlPayload : CreateTaskControlPayload
    {
        /// <summary>
        /// Creates a timestamped message for register TaskControl.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the TaskControl register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.Cuttlefish.TaskControl.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents the payload of the TaskControl register.
    /// </summary>
    public struct TaskControlPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskControlPayload"/> structure.
        /// </summary>
        /// <param name="clearAllTasks">Halts and clears all tasks.</param>
        /// <param name="dumpAllTasks">Sends an event from PwmTask register per currently configured task. Once all events have been sent, a write message will be returned from this register.</param>
        /// <param name="taskCount">Number of tasks currently configured. This portiion of the register is read-only.</param>
        public TaskControlPayload(
            EnableFlag clearAllTasks,
            EnableFlag dumpAllTasks,
            byte taskCount)
        {
            ClearAllTasks = clearAllTasks;
            DumpAllTasks = dumpAllTasks;
            TaskCount = taskCount;
        }

        /// <summary>
        /// Halts and clears all tasks.
        /// </summary>
        public EnableFlag ClearAllTasks;

        /// <summary>
        /// Sends an event from PwmTask register per currently configured task. Once all events have been sent, a write message will be returned from this register.
        /// </summary>
        public EnableFlag DumpAllTasks;

        /// <summary>
        /// Number of tasks currently configured. This portiion of the register is read-only.
        /// </summary>
        public byte TaskCount;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the payload of
        /// the TaskControl register.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the payload of the
        /// TaskControl register.
        /// </returns>
        public override string ToString()
        {
            return "TaskControlPayload { " +
                "ClearAllTasks = " + ClearAllTasks + ", " +
                "DumpAllTasks = " + DumpAllTasks + ", " +
                "TaskCount = " + TaskCount + " " +
            "}";
        }
    }

    /// <summary>
    /// Available ports on the device
    /// </summary>
    [Flags]
    public enum Ports : byte
    {
        None = 0x0,
        Port0 = 0x1,
        Port1 = 0x2,
        Port2 = 0x4,
        Port3 = 0x8,
        Port4 = 0x10,
        Port5 = 0x20,
        Port6 = 0x40,
        Port7 = 0x80
    }
}
