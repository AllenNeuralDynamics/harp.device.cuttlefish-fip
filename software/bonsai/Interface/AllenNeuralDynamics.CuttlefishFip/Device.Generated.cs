using Bonsai;
using Bonsai.Harp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace AllenNeuralDynamics.CuttlefishFip
{
    /// <summary>
    /// Generates events and processes commands for the CuttlefishFip device connected
    /// at the specified serial port.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Generates events and processes commands for the CuttlefishFip device.")]
    public partial class Device : Bonsai.Harp.Device, INamedElement
    {
        /// <summary>
        /// Represents the unique identity class of the <see cref="CuttlefishFip"/> device.
        /// This field is constant.
        /// </summary>
        public const int WhoAmI = 1407;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device() : base(WhoAmI) { }

        string INamedElement.Name => nameof(CuttlefishFip);

        /// <summary>
        /// Gets a read-only mapping from address to register type.
        /// </summary>
        public static new IReadOnlyDictionary<int, Type> RegisterMap { get; } = new Dictionary<int, Type>
            (Bonsai.Harp.Device.RegisterMap.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            { 32, typeof(SetTaskState) },
            { 33, typeof(AddTask) },
            { 34, typeof(RemoveTask) },
            { 35, typeof(ClearAllTasks) },
            { 36, typeof(TaskCount) },
            { 37, typeof(TaskRisingEdgeEvent) },
            { 38, typeof(Task0Settings) },
            { 39, typeof(Task1Settings) },
            { 40, typeof(Task2Settings) },
            { 41, typeof(Task3Settings) },
            { 42, typeof(Task4Settings) },
            { 43, typeof(Task5Settings) },
            { 44, typeof(Task6Settings) },
            { 45, typeof(Task7Settings) }
        };

        /// <summary>
        /// Gets the contents of the metadata file describing the <see cref="CuttlefishFip"/>
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
    /// describing the <see cref="CuttlefishFip"/> device registers.
    /// </summary>
    [Description("Returns the contents of the metadata file describing the CuttlefishFip device registers.")]
    public partial class GetDeviceMetadata : Source<string>
    {
        /// <summary>
        /// Returns an observable sequence with the contents of the metadata file
        /// describing the <see cref="CuttlefishFip"/> device registers.
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
    /// Represents an operator that groups the sequence of <see cref="CuttlefishFip"/>" messages by register type.
    /// </summary>
    [Description("Groups the sequence of CuttlefishFip messages by register type.")]
    public partial class GroupByRegister : Combinator<HarpMessage, IGroupedObservable<Type, HarpMessage>>
    {
        /// <summary>
        /// Groups an observable sequence of <see cref="CuttlefishFip"/> messages
        /// by register type.
        /// </summary>
        /// <param name="source">The sequence of Harp device messages.</param>
        /// <returns>
        /// A sequence of observable groups, each of which corresponds to a unique
        /// <see cref="CuttlefishFip"/> register.
        /// </returns>
        public override IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<HarpMessage> source)
        {
            return source.GroupBy(message => Device.RegisterMap[message.Address]);
        }
    }

    /// <summary>
    /// Represents an operator that writes the sequence of <see cref="CuttlefishFip"/>" messages
    /// to the standard Harp storage format.
    /// </summary>
    [Description("Writes the sequence of CuttlefishFip messages to the standard Harp storage format.")]
    public partial class DeviceDataWriter : Sink<HarpMessage>, INamedElement
    {
        const string BinaryExtension = ".bin";
        const string MetadataFileName = "device.yml";
        readonly Bonsai.Harp.MessageWriter writer = new();

        string INamedElement.Name => nameof(CuttlefishFip) + "DataWriter";

        /// <summary>
        /// Gets or sets the relative or absolute path on which to save the message data.
        /// </summary>
        [Description("The relative or absolute path of the directory on which to save the message data.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path
        {
            get => System.IO.Path.GetDirectoryName(writer.FileName);
            set => writer.FileName = System.IO.Path.Combine(value, nameof(CuttlefishFip) + BinaryExtension);
        }

        /// <summary>
        /// Gets or sets a value indicating whether element writing should be buffered. If <see langword="true"/>,
        /// the write commands will be queued in memory as fast as possible and will be processed
        /// by the writer in a different thread. Otherwise, writing will be done in the same
        /// thread in which notifications arrive.
        /// </summary>
        [Description("Indicates whether writing should be buffered.")]
        public bool Buffered
        {
            get => writer.Buffered;
            set => writer.Buffered = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output file if it already exists.
        /// </summary>
        [Description("Indicates whether to overwrite the output file if it already exists.")]
        public bool Overwrite
        {
            get => writer.Overwrite;
            set => writer.Overwrite = value;
        }

        /// <summary>
        /// Gets or sets a value specifying how the message filter will use the matching criteria.
        /// </summary>
        [Description("Specifies how the message filter will use the matching criteria.")]
        public FilterType FilterType
        {
            get => writer.FilterType;
            set => writer.FilterType = value;
        }

        /// <summary>
        /// Gets or sets a value specifying the expected message type. If no value is
        /// specified, all messages will be accepted.
        /// </summary>
        [Description("Specifies the expected message type. If no value is specified, all messages will be accepted.")]
        public MessageType? MessageType
        {
            get => writer.MessageType;
            set => writer.MessageType = value;
        }

        private IObservable<TSource> WriteDeviceMetadata<TSource>(IObservable<TSource> source)
        {
            var basePath = Path;
            if (string.IsNullOrEmpty(basePath))
                return source;

            var metadataPath = System.IO.Path.Combine(basePath, MetadataFileName);
            return Observable.Create<TSource>(observer =>
            {
                Bonsai.IO.PathHelper.EnsureDirectory(metadataPath);
                if (System.IO.File.Exists(metadataPath) && !Overwrite)
                {
                    throw new System.IO.IOException(string.Format("The file '{0}' already exists.", metadataPath));
                }

                System.IO.File.WriteAllText(metadataPath, Device.Metadata);
                return source.SubscribeSafe(observer);
            });
        }

        /// <summary>
        /// Writes each Harp message in the sequence to the specified binary file, and the
        /// contents of the device metadata file to a separate text file.
        /// </summary>
        /// <param name="source">The sequence of messages to write to the file.</param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the
        /// messages to a raw binary file, and the contents of the device metadata file
        /// to a separate text file.
        /// </returns>
        public override IObservable<HarpMessage> Process(IObservable<HarpMessage> source)
        {
            return source.Publish(ps => ps.Merge(
                WriteDeviceMetadata(writer.Process(ps.GroupBy(message => message.Address)))
                .IgnoreElements()
                .Cast<HarpMessage>()));
        }

        /// <summary>
        /// Writes each Harp message in the sequence of observable groups to the
        /// corresponding binary file, where the name of each file is generated from
        /// the common group register address. The contents of the device metadata file are
        /// written to a separate text file.
        /// </summary>
        /// <param name="source">
        /// A sequence of observable groups, each of which corresponds to a unique register
        /// address.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the Harp
        /// messages in each group to the corresponding file, and the contents of the device
        /// metadata file to a separate text file.
        /// </returns>
        public IObservable<IGroupedObservable<int, HarpMessage>> Process(IObservable<IGroupedObservable<int, HarpMessage>> source)
        {
            return WriteDeviceMetadata(writer.Process(source));
        }

        /// <summary>
        /// Writes each Harp message in the sequence of observable groups to the
        /// corresponding binary file, where the name of each file is generated from
        /// the common group register name. The contents of the device metadata file are
        /// written to a separate text file.
        /// </summary>
        /// <param name="source">
        /// A sequence of observable groups, each of which corresponds to a unique register
        /// type.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the Harp
        /// messages in each group to the corresponding file, and the contents of the device
        /// metadata file to a separate text file.
        /// </returns>
        public IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<IGroupedObservable<Type, HarpMessage>> source)
        {
            return WriteDeviceMetadata(writer.Process(source));
        }
    }

    /// <summary>
    /// Represents an operator that filters register-specific messages
    /// reported by the <see cref="CuttlefishFip"/> device.
    /// </summary>
    /// <seealso cref="SetTaskState"/>
    /// <seealso cref="AddTask"/>
    /// <seealso cref="RemoveTask"/>
    /// <seealso cref="ClearAllTasks"/>
    /// <seealso cref="TaskCount"/>
    /// <seealso cref="TaskRisingEdgeEvent"/>
    /// <seealso cref="Task0Settings"/>
    /// <seealso cref="Task1Settings"/>
    /// <seealso cref="Task2Settings"/>
    /// <seealso cref="Task3Settings"/>
    /// <seealso cref="Task4Settings"/>
    /// <seealso cref="Task5Settings"/>
    /// <seealso cref="Task6Settings"/>
    /// <seealso cref="Task7Settings"/>
    [XmlInclude(typeof(SetTaskState))]
    [XmlInclude(typeof(AddTask))]
    [XmlInclude(typeof(RemoveTask))]
    [XmlInclude(typeof(ClearAllTasks))]
    [XmlInclude(typeof(TaskCount))]
    [XmlInclude(typeof(TaskRisingEdgeEvent))]
    [XmlInclude(typeof(Task0Settings))]
    [XmlInclude(typeof(Task1Settings))]
    [XmlInclude(typeof(Task2Settings))]
    [XmlInclude(typeof(Task3Settings))]
    [XmlInclude(typeof(Task4Settings))]
    [XmlInclude(typeof(Task5Settings))]
    [XmlInclude(typeof(Task6Settings))]
    [XmlInclude(typeof(Task7Settings))]
    [Description("Filters register-specific messages reported by the CuttlefishFip device.")]
    public class FilterRegister : FilterRegisterBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterRegister"/> class.
        /// </summary>
        public FilterRegister()
        {
            Register = new SetTaskState();
        }

        string INamedElement.Name
        {
            get => $"{nameof(CuttlefishFip)}.{GetElementDisplayName(Register)}";
        }
    }

    /// <summary>
    /// Represents an operator which filters and selects specific messages
    /// reported by the CuttlefishFip device.
    /// </summary>
    /// <seealso cref="SetTaskState"/>
    /// <seealso cref="AddTask"/>
    /// <seealso cref="RemoveTask"/>
    /// <seealso cref="ClearAllTasks"/>
    /// <seealso cref="TaskCount"/>
    /// <seealso cref="TaskRisingEdgeEvent"/>
    /// <seealso cref="Task0Settings"/>
    /// <seealso cref="Task1Settings"/>
    /// <seealso cref="Task2Settings"/>
    /// <seealso cref="Task3Settings"/>
    /// <seealso cref="Task4Settings"/>
    /// <seealso cref="Task5Settings"/>
    /// <seealso cref="Task6Settings"/>
    /// <seealso cref="Task7Settings"/>
    [XmlInclude(typeof(SetTaskState))]
    [XmlInclude(typeof(AddTask))]
    [XmlInclude(typeof(RemoveTask))]
    [XmlInclude(typeof(ClearAllTasks))]
    [XmlInclude(typeof(TaskCount))]
    [XmlInclude(typeof(TaskRisingEdgeEvent))]
    [XmlInclude(typeof(Task0Settings))]
    [XmlInclude(typeof(Task1Settings))]
    [XmlInclude(typeof(Task2Settings))]
    [XmlInclude(typeof(Task3Settings))]
    [XmlInclude(typeof(Task4Settings))]
    [XmlInclude(typeof(Task5Settings))]
    [XmlInclude(typeof(Task6Settings))]
    [XmlInclude(typeof(Task7Settings))]
    [XmlInclude(typeof(TimestampedSetTaskState))]
    [XmlInclude(typeof(TimestampedAddTask))]
    [XmlInclude(typeof(TimestampedRemoveTask))]
    [XmlInclude(typeof(TimestampedClearAllTasks))]
    [XmlInclude(typeof(TimestampedTaskCount))]
    [XmlInclude(typeof(TimestampedTaskRisingEdgeEvent))]
    [XmlInclude(typeof(TimestampedTask0Settings))]
    [XmlInclude(typeof(TimestampedTask1Settings))]
    [XmlInclude(typeof(TimestampedTask2Settings))]
    [XmlInclude(typeof(TimestampedTask3Settings))]
    [XmlInclude(typeof(TimestampedTask4Settings))]
    [XmlInclude(typeof(TimestampedTask5Settings))]
    [XmlInclude(typeof(TimestampedTask6Settings))]
    [XmlInclude(typeof(TimestampedTask7Settings))]
    [Description("Filters and selects specific messages reported by the CuttlefishFip device.")]
    public partial class Parse : ParseBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parse"/> class.
        /// </summary>
        public Parse()
        {
            Register = new SetTaskState();
        }

        string INamedElement.Name => $"{nameof(CuttlefishFip)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents an operator which formats a sequence of values as specific
    /// CuttlefishFip register messages.
    /// </summary>
    /// <seealso cref="SetTaskState"/>
    /// <seealso cref="AddTask"/>
    /// <seealso cref="RemoveTask"/>
    /// <seealso cref="ClearAllTasks"/>
    /// <seealso cref="TaskCount"/>
    /// <seealso cref="TaskRisingEdgeEvent"/>
    /// <seealso cref="Task0Settings"/>
    /// <seealso cref="Task1Settings"/>
    /// <seealso cref="Task2Settings"/>
    /// <seealso cref="Task3Settings"/>
    /// <seealso cref="Task4Settings"/>
    /// <seealso cref="Task5Settings"/>
    /// <seealso cref="Task6Settings"/>
    /// <seealso cref="Task7Settings"/>
    [XmlInclude(typeof(SetTaskState))]
    [XmlInclude(typeof(AddTask))]
    [XmlInclude(typeof(RemoveTask))]
    [XmlInclude(typeof(ClearAllTasks))]
    [XmlInclude(typeof(TaskCount))]
    [XmlInclude(typeof(TaskRisingEdgeEvent))]
    [XmlInclude(typeof(Task0Settings))]
    [XmlInclude(typeof(Task1Settings))]
    [XmlInclude(typeof(Task2Settings))]
    [XmlInclude(typeof(Task3Settings))]
    [XmlInclude(typeof(Task4Settings))]
    [XmlInclude(typeof(Task5Settings))]
    [XmlInclude(typeof(Task6Settings))]
    [XmlInclude(typeof(Task7Settings))]
    [Description("Formats a sequence of values as specific CuttlefishFip register messages.")]
    public partial class Format : FormatBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Format"/> class.
        /// </summary>
        public Format()
        {
            Register = new SetTaskState();
        }

        string INamedElement.Name => $"{nameof(CuttlefishFip)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents a register that starts/Stops all scheduled tasks.
    /// </summary>
    [Description("Starts/Stops all scheduled tasks")]
    public partial class SetTaskState
    {
        /// <summary>
        /// Represents the address of the <see cref="SetTaskState"/> register. This field is constant.
        /// </summary>
        public const int Address = 32;

        /// <summary>
        /// Represents the payload type of the <see cref="SetTaskState"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="SetTaskState"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="SetTaskState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static TaskState GetPayload(HarpMessage message)
        {
            return (TaskState)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="SetTaskState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<TaskState> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((TaskState)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="SetTaskState"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SetTaskState"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, TaskState value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="SetTaskState"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SetTaskState"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, TaskState value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// SetTaskState register.
    /// </summary>
    /// <seealso cref="SetTaskState"/>
    [Description("Filters and selects timestamped messages from the SetTaskState register.")]
    public partial class TimestampedSetTaskState
    {
        /// <summary>
        /// Represents the address of the <see cref="SetTaskState"/> register. This field is constant.
        /// </summary>
        public const int Address = SetTaskState.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="SetTaskState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<TaskState> GetPayload(HarpMessage message)
        {
            return SetTaskState.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).
    /// </summary>
    [Description("Schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us)")]
    public partial class AddTask
    {
        /// <summary>
        /// Represents the address of the <see cref="AddTask"/> register. This field is constant.
        /// </summary>
        public const int Address = 33;

        /// <summary>
        /// Represents the payload type of the <see cref="AddTask"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="AddTask"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="AddTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="AddTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="AddTask"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AddTask"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="AddTask"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AddTask"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// AddTask register.
    /// </summary>
    /// <seealso cref="AddTask"/>
    [Description("Filters and selects timestamped messages from the AddTask register.")]
    public partial class TimestampedAddTask
    {
        /// <summary>
        /// Represents the address of the <see cref="AddTask"/> register. This field is constant.
        /// </summary>
        public const int Address = AddTask.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="AddTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return AddTask.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.
    /// </summary>
    [Description("Removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned")]
    public partial class RemoveTask
    {
        /// <summary>
        /// Represents the address of the <see cref="RemoveTask"/> register. This field is constant.
        /// </summary>
        public const int Address = 34;

        /// <summary>
        /// Represents the payload type of the <see cref="RemoveTask"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="RemoveTask"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="RemoveTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static TaskIndex GetPayload(HarpMessage message)
        {
            return (TaskIndex)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="RemoveTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<TaskIndex> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((TaskIndex)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="RemoveTask"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="RemoveTask"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, TaskIndex value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="RemoveTask"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="RemoveTask"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, TaskIndex value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// RemoveTask register.
    /// </summary>
    /// <seealso cref="RemoveTask"/>
    [Description("Filters and selects timestamped messages from the RemoveTask register.")]
    public partial class TimestampedRemoveTask
    {
        /// <summary>
        /// Represents the address of the <see cref="RemoveTask"/> register. This field is constant.
        /// </summary>
        public const int Address = RemoveTask.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="RemoveTask"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<TaskIndex> GetPayload(HarpMessage message)
        {
            return RemoveTask.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that clears all scheduled task if a value of 1 is written.
    /// </summary>
    [Description("Clears all scheduled task if a value of 1 is written.")]
    public partial class ClearAllTasks
    {
        /// <summary>
        /// Represents the address of the <see cref="ClearAllTasks"/> register. This field is constant.
        /// </summary>
        public const int Address = 35;

        /// <summary>
        /// Represents the payload type of the <see cref="ClearAllTasks"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="ClearAllTasks"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="ClearAllTasks"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static EnableFlag GetPayload(HarpMessage message)
        {
            return (EnableFlag)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="ClearAllTasks"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<EnableFlag> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((EnableFlag)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="ClearAllTasks"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ClearAllTasks"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, EnableFlag value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="ClearAllTasks"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ClearAllTasks"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, EnableFlag value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// ClearAllTasks register.
    /// </summary>
    /// <seealso cref="ClearAllTasks"/>
    [Description("Filters and selects timestamped messages from the ClearAllTasks register.")]
    public partial class TimestampedClearAllTasks
    {
        /// <summary>
        /// Represents the address of the <see cref="ClearAllTasks"/> register. This field is constant.
        /// </summary>
        public const int Address = ClearAllTasks.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="ClearAllTasks"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<EnableFlag> GetPayload(HarpMessage message)
        {
            return ClearAllTasks.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that returns the number of tasks currently scheduled. This register is read-only.
    /// </summary>
    [Description("Returns the number of tasks currently scheduled. This register is read-only.")]
    public partial class TaskCount
    {
        /// <summary>
        /// Represents the address of the <see cref="TaskCount"/> register. This field is constant.
        /// </summary>
        public const int Address = 36;

        /// <summary>
        /// Represents the payload type of the <see cref="TaskCount"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="TaskCount"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="TaskCount"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="TaskCount"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="TaskCount"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TaskCount"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="TaskCount"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TaskCount"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// TaskCount register.
    /// </summary>
    /// <seealso cref="TaskCount"/>
    [Description("Filters and selects timestamped messages from the TaskCount register.")]
    public partial class TimestampedTaskCount
    {
        /// <summary>
        /// Represents the address of the <see cref="TaskCount"/> register. This field is constant.
        /// </summary>
        public const int Address = TaskCount.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="TaskCount"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return TaskCount.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.
    /// </summary>
    [Description("An event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.")]
    public partial class TaskRisingEdgeEvent
    {
        /// <summary>
        /// Represents the address of the <see cref="TaskRisingEdgeEvent"/> register. This field is constant.
        /// </summary>
        public const int Address = 37;

        /// <summary>
        /// Represents the payload type of the <see cref="TaskRisingEdgeEvent"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="TaskRisingEdgeEvent"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="TaskRisingEdgeEvent"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Ports GetPayload(HarpMessage message)
        {
            return (Ports)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="TaskRisingEdgeEvent"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Ports)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="TaskRisingEdgeEvent"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TaskRisingEdgeEvent"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="TaskRisingEdgeEvent"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TaskRisingEdgeEvent"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Ports value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// TaskRisingEdgeEvent register.
    /// </summary>
    /// <seealso cref="TaskRisingEdgeEvent"/>
    [Description("Filters and selects timestamped messages from the TaskRisingEdgeEvent register.")]
    public partial class TimestampedTaskRisingEdgeEvent
    {
        /// <summary>
        /// Represents the address of the <see cref="TaskRisingEdgeEvent"/> register. This field is constant.
        /// </summary>
        public const int Address = TaskRisingEdgeEvent.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="TaskRisingEdgeEvent"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Ports> GetPayload(HarpMessage message)
        {
            return TaskRisingEdgeEvent.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task0.
    /// </summary>
    [Description("Represents the settings of Task0.")]
    public partial class Task0Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task0Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 38;

        /// <summary>
        /// Represents the payload type of the <see cref="Task0Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task0Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task0Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task0Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task0Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task0Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task0Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task0Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task0Settings register.
    /// </summary>
    /// <seealso cref="Task0Settings"/>
    [Description("Filters and selects timestamped messages from the Task0Settings register.")]
    public partial class TimestampedTask0Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task0Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task0Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task0Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task0Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task1.
    /// </summary>
    [Description("Represents the settings of Task1.")]
    public partial class Task1Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task1Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 39;

        /// <summary>
        /// Represents the payload type of the <see cref="Task1Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task1Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task1Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task1Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task1Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task1Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task1Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task1Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task1Settings register.
    /// </summary>
    /// <seealso cref="Task1Settings"/>
    [Description("Filters and selects timestamped messages from the Task1Settings register.")]
    public partial class TimestampedTask1Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task1Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task1Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task1Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task1Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task2.
    /// </summary>
    [Description("Represents the settings of Task2.")]
    public partial class Task2Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task2Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 40;

        /// <summary>
        /// Represents the payload type of the <see cref="Task2Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task2Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task2Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task2Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task2Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task2Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task2Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task2Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task2Settings register.
    /// </summary>
    /// <seealso cref="Task2Settings"/>
    [Description("Filters and selects timestamped messages from the Task2Settings register.")]
    public partial class TimestampedTask2Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task2Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task2Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task2Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task2Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task3.
    /// </summary>
    [Description("Represents the settings of Task3.")]
    public partial class Task3Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task3Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 41;

        /// <summary>
        /// Represents the payload type of the <see cref="Task3Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task3Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task3Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task3Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task3Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task3Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task3Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task3Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task3Settings register.
    /// </summary>
    /// <seealso cref="Task3Settings"/>
    [Description("Filters and selects timestamped messages from the Task3Settings register.")]
    public partial class TimestampedTask3Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task3Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task3Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task3Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task3Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task4.
    /// </summary>
    [Description("Represents the settings of Task4.")]
    public partial class Task4Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task4Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 42;

        /// <summary>
        /// Represents the payload type of the <see cref="Task4Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task4Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task4Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task4Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task4Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task4Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task4Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task4Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task4Settings register.
    /// </summary>
    /// <seealso cref="Task4Settings"/>
    [Description("Filters and selects timestamped messages from the Task4Settings register.")]
    public partial class TimestampedTask4Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task4Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task4Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task4Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task4Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task5.
    /// </summary>
    [Description("Represents the settings of Task5.")]
    public partial class Task5Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task5Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 43;

        /// <summary>
        /// Represents the payload type of the <see cref="Task5Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task5Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task5Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task5Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task5Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task5Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task5Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task5Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task5Settings register.
    /// </summary>
    /// <seealso cref="Task5Settings"/>
    [Description("Filters and selects timestamped messages from the Task5Settings register.")]
    public partial class TimestampedTask5Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task5Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task5Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task5Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task5Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task6.
    /// </summary>
    [Description("Represents the settings of Task6.")]
    public partial class Task6Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task6Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 44;

        /// <summary>
        /// Represents the payload type of the <see cref="Task6Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task6Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task6Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task6Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task6Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task6Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task6Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task6Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task6Settings register.
    /// </summary>
    /// <seealso cref="Task6Settings"/>
    [Description("Filters and selects timestamped messages from the Task6Settings register.")]
    public partial class TimestampedTask6Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task6Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task6Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task6Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task6Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that represents the settings of Task7.
    /// </summary>
    [Description("Represents the settings of Task7.")]
    public partial class Task7Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task7Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = 45;

        /// <summary>
        /// Represents the payload type of the <see cref="Task7Settings"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Task7Settings"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 34;

        /// <summary>
        /// Returns the payload data for <see cref="Task7Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte[] GetPayload(HarpMessage message)
        {
            return message.GetPayloadArray<byte>();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Task7Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadArray<byte>();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Task7Settings"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task7Settings"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Task7Settings"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Task7Settings"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte[] value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Task7Settings register.
    /// </summary>
    /// <seealso cref="Task7Settings"/>
    [Description("Filters and selects timestamped messages from the Task7Settings register.")]
    public partial class TimestampedTask7Settings
    {
        /// <summary>
        /// Represents the address of the <see cref="Task7Settings"/> register. This field is constant.
        /// </summary>
        public const int Address = Task7Settings.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Task7Settings"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte[]> GetPayload(HarpMessage message)
        {
            return Task7Settings.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents an operator which creates standard message payloads for the
    /// CuttlefishFip device.
    /// </summary>
    /// <seealso cref="CreateSetTaskStatePayload"/>
    /// <seealso cref="CreateAddTaskPayload"/>
    /// <seealso cref="CreateRemoveTaskPayload"/>
    /// <seealso cref="CreateClearAllTasksPayload"/>
    /// <seealso cref="CreateTaskCountPayload"/>
    /// <seealso cref="CreateTaskRisingEdgeEventPayload"/>
    /// <seealso cref="CreateTask0SettingsPayload"/>
    /// <seealso cref="CreateTask1SettingsPayload"/>
    /// <seealso cref="CreateTask2SettingsPayload"/>
    /// <seealso cref="CreateTask3SettingsPayload"/>
    /// <seealso cref="CreateTask4SettingsPayload"/>
    /// <seealso cref="CreateTask5SettingsPayload"/>
    /// <seealso cref="CreateTask6SettingsPayload"/>
    /// <seealso cref="CreateTask7SettingsPayload"/>
    [XmlInclude(typeof(CreateSetTaskStatePayload))]
    [XmlInclude(typeof(CreateAddTaskPayload))]
    [XmlInclude(typeof(CreateRemoveTaskPayload))]
    [XmlInclude(typeof(CreateClearAllTasksPayload))]
    [XmlInclude(typeof(CreateTaskCountPayload))]
    [XmlInclude(typeof(CreateTaskRisingEdgeEventPayload))]
    [XmlInclude(typeof(CreateTask0SettingsPayload))]
    [XmlInclude(typeof(CreateTask1SettingsPayload))]
    [XmlInclude(typeof(CreateTask2SettingsPayload))]
    [XmlInclude(typeof(CreateTask3SettingsPayload))]
    [XmlInclude(typeof(CreateTask4SettingsPayload))]
    [XmlInclude(typeof(CreateTask5SettingsPayload))]
    [XmlInclude(typeof(CreateTask6SettingsPayload))]
    [XmlInclude(typeof(CreateTask7SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedSetTaskStatePayload))]
    [XmlInclude(typeof(CreateTimestampedAddTaskPayload))]
    [XmlInclude(typeof(CreateTimestampedRemoveTaskPayload))]
    [XmlInclude(typeof(CreateTimestampedClearAllTasksPayload))]
    [XmlInclude(typeof(CreateTimestampedTaskCountPayload))]
    [XmlInclude(typeof(CreateTimestampedTaskRisingEdgeEventPayload))]
    [XmlInclude(typeof(CreateTimestampedTask0SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedTask1SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedTask2SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedTask3SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedTask4SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedTask5SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedTask6SettingsPayload))]
    [XmlInclude(typeof(CreateTimestampedTask7SettingsPayload))]
    [Description("Creates standard message payloads for the CuttlefishFip device.")]
    public partial class CreateMessage : CreateMessageBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMessage"/> class.
        /// </summary>
        public CreateMessage()
        {
            Payload = new CreateSetTaskStatePayload();
        }

        string INamedElement.Name => $"{nameof(CuttlefishFip)}.{GetElementDisplayName(Payload)}";
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that starts/Stops all scheduled tasks.
    /// </summary>
    [DisplayName("SetTaskStatePayload")]
    [Description("Creates a message payload that starts/Stops all scheduled tasks.")]
    public partial class CreateSetTaskStatePayload
    {
        /// <summary>
        /// Gets or sets the value that starts/Stops all scheduled tasks.
        /// </summary>
        [Description("The value that starts/Stops all scheduled tasks.")]
        public TaskState SetTaskState { get; set; }

        /// <summary>
        /// Creates a message payload for the SetTaskState register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public TaskState GetPayload()
        {
            return SetTaskState;
        }

        /// <summary>
        /// Creates a message that starts/Stops all scheduled tasks.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the SetTaskState register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.SetTaskState.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that starts/Stops all scheduled tasks.
    /// </summary>
    [DisplayName("TimestampedSetTaskStatePayload")]
    [Description("Creates a timestamped message payload that starts/Stops all scheduled tasks.")]
    public partial class CreateTimestampedSetTaskStatePayload : CreateSetTaskStatePayload
    {
        /// <summary>
        /// Creates a timestamped message that starts/Stops all scheduled tasks.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the SetTaskState register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.SetTaskState.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).
    /// </summary>
    [DisplayName("AddTaskPayload")]
    [Description("Creates a message payload that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).")]
    public partial class CreateAddTaskPayload
    {
        /// <summary>
        /// Gets or sets the value that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).
        /// </summary>
        [Description("The value that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).")]
        public byte[] AddTask { get; set; }

        /// <summary>
        /// Creates a message payload for the AddTask register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return AddTask;
        }

        /// <summary>
        /// Creates a message that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the AddTask register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.AddTask.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).
    /// </summary>
    [DisplayName("TimestampedAddTaskPayload")]
    [Description("Creates a timestamped message payload that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).")]
    public partial class CreateTimestampedAddTaskPayload : CreateAddTaskPayload
    {
        /// <summary>
        /// Creates a timestamped message that schedules a task by modelling following structure: U32 IOPin, float DutyCycle(0-1), float Frequency(Hz), U32 OutputMask (IOPins), U8 Events (0/1), U8 Mute (Kill the output but preserves timing), u32 delta1-4 (us).
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the AddTask register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.AddTask.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.
    /// </summary>
    [DisplayName("RemoveTaskPayload")]
    [Description("Creates a message payload that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.")]
    public partial class CreateRemoveTaskPayload
    {
        /// <summary>
        /// Gets or sets the value that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.
        /// </summary>
        [Description("The value that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.")]
        public TaskIndex RemoveTask { get; set; }

        /// <summary>
        /// Creates a message payload for the RemoveTask register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public TaskIndex GetPayload()
        {
            return RemoveTask;
        }

        /// <summary>
        /// Creates a message that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the RemoveTask register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.RemoveTask.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.
    /// </summary>
    [DisplayName("TimestampedRemoveTaskPayload")]
    [Description("Creates a timestamped message payload that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.")]
    public partial class CreateTimestampedRemoveTaskPayload : CreateRemoveTaskPayload
    {
        /// <summary>
        /// Creates a timestamped message that removes a task scheduled at index 0-7. If the task is not scheduled, an error will be returned.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the RemoveTask register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.RemoveTask.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that clears all scheduled task if a value of 1 is written.
    /// </summary>
    [DisplayName("ClearAllTasksPayload")]
    [Description("Creates a message payload that clears all scheduled task if a value of 1 is written.")]
    public partial class CreateClearAllTasksPayload
    {
        /// <summary>
        /// Gets or sets the value that clears all scheduled task if a value of 1 is written.
        /// </summary>
        [Description("The value that clears all scheduled task if a value of 1 is written.")]
        public EnableFlag ClearAllTasks { get; set; }

        /// <summary>
        /// Creates a message payload for the ClearAllTasks register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public EnableFlag GetPayload()
        {
            return ClearAllTasks;
        }

        /// <summary>
        /// Creates a message that clears all scheduled task if a value of 1 is written.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the ClearAllTasks register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.ClearAllTasks.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that clears all scheduled task if a value of 1 is written.
    /// </summary>
    [DisplayName("TimestampedClearAllTasksPayload")]
    [Description("Creates a timestamped message payload that clears all scheduled task if a value of 1 is written.")]
    public partial class CreateTimestampedClearAllTasksPayload : CreateClearAllTasksPayload
    {
        /// <summary>
        /// Creates a timestamped message that clears all scheduled task if a value of 1 is written.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the ClearAllTasks register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.ClearAllTasks.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that returns the number of tasks currently scheduled. This register is read-only.
    /// </summary>
    [DisplayName("TaskCountPayload")]
    [Description("Creates a message payload that returns the number of tasks currently scheduled. This register is read-only.")]
    public partial class CreateTaskCountPayload
    {
        /// <summary>
        /// Gets or sets the value that returns the number of tasks currently scheduled. This register is read-only.
        /// </summary>
        [Description("The value that returns the number of tasks currently scheduled. This register is read-only.")]
        public byte TaskCount { get; set; }

        /// <summary>
        /// Creates a message payload for the TaskCount register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return TaskCount;
        }

        /// <summary>
        /// Creates a message that returns the number of tasks currently scheduled. This register is read-only.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the TaskCount register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.TaskCount.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that returns the number of tasks currently scheduled. This register is read-only.
    /// </summary>
    [DisplayName("TimestampedTaskCountPayload")]
    [Description("Creates a timestamped message payload that returns the number of tasks currently scheduled. This register is read-only.")]
    public partial class CreateTimestampedTaskCountPayload : CreateTaskCountPayload
    {
        /// <summary>
        /// Creates a timestamped message that returns the number of tasks currently scheduled. This register is read-only.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the TaskCount register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.TaskCount.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.
    /// </summary>
    [DisplayName("TaskRisingEdgeEventPayload")]
    [Description("Creates a message payload that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.")]
    public partial class CreateTaskRisingEdgeEventPayload
    {
        /// <summary>
        /// Gets or sets the value that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.
        /// </summary>
        [Description("The value that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.")]
        public Ports TaskRisingEdgeEvent { get; set; }

        /// <summary>
        /// Creates a message payload for the TaskRisingEdgeEvent register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Ports GetPayload()
        {
            return TaskRisingEdgeEvent;
        }

        /// <summary>
        /// Creates a message that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the TaskRisingEdgeEvent register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.TaskRisingEdgeEvent.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.
    /// </summary>
    [DisplayName("TimestampedTaskRisingEdgeEventPayload")]
    [Description("Creates a timestamped message payload that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.")]
    public partial class CreateTimestampedTaskRisingEdgeEventPayload : CreateTaskRisingEdgeEventPayload
    {
        /// <summary>
        /// Creates a timestamped message that an event raised when a rising edge of any of the ports is detected. The `Events` flag must be enabled in the corresponding task to trigger this event. The event is raised when the task is started. The event is cleared when the task is removed or stopped.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the TaskRisingEdgeEvent register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.TaskRisingEdgeEvent.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task0.
    /// </summary>
    [DisplayName("Task0SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task0.")]
    public partial class CreateTask0SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task0.
        /// </summary>
        [Description("The value that represents the settings of Task0.")]
        public byte[] Task0Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task0Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task0Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task0.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task0Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task0Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task0.
    /// </summary>
    [DisplayName("TimestampedTask0SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task0.")]
    public partial class CreateTimestampedTask0SettingsPayload : CreateTask0SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task0.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task0Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task0Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task1.
    /// </summary>
    [DisplayName("Task1SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task1.")]
    public partial class CreateTask1SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task1.
        /// </summary>
        [Description("The value that represents the settings of Task1.")]
        public byte[] Task1Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task1Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task1Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task1.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task1Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task1Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task1.
    /// </summary>
    [DisplayName("TimestampedTask1SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task1.")]
    public partial class CreateTimestampedTask1SettingsPayload : CreateTask1SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task1.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task1Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task1Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task2.
    /// </summary>
    [DisplayName("Task2SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task2.")]
    public partial class CreateTask2SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task2.
        /// </summary>
        [Description("The value that represents the settings of Task2.")]
        public byte[] Task2Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task2Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task2Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task2.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task2Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task2Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task2.
    /// </summary>
    [DisplayName("TimestampedTask2SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task2.")]
    public partial class CreateTimestampedTask2SettingsPayload : CreateTask2SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task2.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task2Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task2Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task3.
    /// </summary>
    [DisplayName("Task3SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task3.")]
    public partial class CreateTask3SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task3.
        /// </summary>
        [Description("The value that represents the settings of Task3.")]
        public byte[] Task3Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task3Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task3Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task3.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task3Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task3Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task3.
    /// </summary>
    [DisplayName("TimestampedTask3SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task3.")]
    public partial class CreateTimestampedTask3SettingsPayload : CreateTask3SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task3.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task3Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task3Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task4.
    /// </summary>
    [DisplayName("Task4SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task4.")]
    public partial class CreateTask4SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task4.
        /// </summary>
        [Description("The value that represents the settings of Task4.")]
        public byte[] Task4Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task4Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task4Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task4.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task4Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task4Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task4.
    /// </summary>
    [DisplayName("TimestampedTask4SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task4.")]
    public partial class CreateTimestampedTask4SettingsPayload : CreateTask4SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task4.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task4Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task4Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task5.
    /// </summary>
    [DisplayName("Task5SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task5.")]
    public partial class CreateTask5SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task5.
        /// </summary>
        [Description("The value that represents the settings of Task5.")]
        public byte[] Task5Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task5Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task5Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task5.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task5Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task5Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task5.
    /// </summary>
    [DisplayName("TimestampedTask5SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task5.")]
    public partial class CreateTimestampedTask5SettingsPayload : CreateTask5SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task5.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task5Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task5Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task6.
    /// </summary>
    [DisplayName("Task6SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task6.")]
    public partial class CreateTask6SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task6.
        /// </summary>
        [Description("The value that represents the settings of Task6.")]
        public byte[] Task6Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task6Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task6Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task6.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task6Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task6Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task6.
    /// </summary>
    [DisplayName("TimestampedTask6SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task6.")]
    public partial class CreateTimestampedTask6SettingsPayload : CreateTask6SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task6.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task6Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task6Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that represents the settings of Task7.
    /// </summary>
    [DisplayName("Task7SettingsPayload")]
    [Description("Creates a message payload that represents the settings of Task7.")]
    public partial class CreateTask7SettingsPayload
    {
        /// <summary>
        /// Gets or sets the value that represents the settings of Task7.
        /// </summary>
        [Description("The value that represents the settings of Task7.")]
        public byte[] Task7Settings { get; set; }

        /// <summary>
        /// Creates a message payload for the Task7Settings register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte[] GetPayload()
        {
            return Task7Settings;
        }

        /// <summary>
        /// Creates a message that represents the settings of Task7.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Task7Settings register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task7Settings.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that represents the settings of Task7.
    /// </summary>
    [DisplayName("TimestampedTask7SettingsPayload")]
    [Description("Creates a timestamped message payload that represents the settings of Task7.")]
    public partial class CreateTimestampedTask7SettingsPayload : CreateTask7SettingsPayload
    {
        /// <summary>
        /// Creates a timestamped message that represents the settings of Task7.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Task7Settings register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.CuttlefishFip.Task7Settings.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Available ports on the device. This enum is a bit-mask. Multiple values can be set at the same time.
    /// </summary>
    [Flags]
    public enum Ports : byte
    {
        None = 0x0,
        IO0 = 0x1,
        IO1 = 0x2,
        IO2 = 0x4,
        IO3 = 0x8,
        IO4 = 0x10,
        IO5 = 0x20,
        IO6 = 0x40,
        IO7 = 0x80
    }

    /// <summary>
    /// The state of the ongoing task.
    /// </summary>
    public enum TaskState : byte
    {
        Stop = 0,
        Start = 1,
        Abort = 2
    }

    /// <summary>
    /// Task slot to be used for the task. 0-7
    /// </summary>
    public enum TaskIndex : byte
    {
        Task0 = 0,
        Task1 = 1,
        Task2 = 2,
        Task3 = 3,
        Task4 = 4,
        Task5 = 5,
        Task6 = 6,
        Task7 = 7
    }

    /// <summary>
    /// Available ports on the device. This enum is one-hot encoded. Only one value can be set at a time.
    /// </summary>
    public enum Port : byte
    {
        None = 0,
        IO0 = 1,
        IO1 = 2,
        IO2 = 4,
        IO3 = 8,
        IO4 = 16,
        IO5 = 32,
        IO6 = 64,
        IO7 = 128
    }
}
