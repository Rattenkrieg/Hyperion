﻿#region copyright
// -----------------------------------------------------------------------
//  <copyright file="TypicalMessageTest.cs" company="Akka.NET Team">
//      Copyright (C) 2015-2016 AsynkronIT <https://github.com/AsynkronIT>
//      Copyright (C) 2016-2016 Akka.NET Team <https://github.com/akkadotnet>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Hyperion.PerfTest.ManualSerializer;
using Hyperion.PerfTest.Types;

namespace Hyperion.PerfTest.Tests
{
    class CustomTypicalMessage : ICustomBinarySerializable
    {
        public string StringProp { get; set; }
        public int IntProp { get; set; }
        public Guid GuidProp { get; set; }
        public DateTime DateProp { get; set; }

        public void WriteDataTo(BinaryWriter writer)
        {
            writer.Write(StringProp);
            writer.Write(IntProp);
            writer.Write(GuidProp.ToByteArray());
            writer.Write(DateProp.Ticks);
            writer.Write((byte)DateProp.Kind);
        }

        public void SetDataFrom(BinaryReader reader)
        {
            StringProp = reader.ReadString();
            IntProp = reader.ReadInt32();
            var bytes = reader.ReadBytes(16);
            GuidProp= new Guid(bytes);
            var ticks = reader.ReadInt64();
            var kind = reader.ReadByte();
            DateProp = new DateTime(ticks,(DateTimeKind)kind);
        }
    }
    class TypicalMessageTest : TestBase<TypicalMessage>
    {
        protected override TypicalMessage GetValue()
        {
            return new TypicalMessage()
            {
                StringProp = "hello",
                GuidProp = Guid.NewGuid(),
                IntProp = 123,
                DateProp = DateTime.UtcNow
            };
        }

        protected override void TestAll()
        {
            SerializeCustom();
            base.TestAll();
        }

        private void SerializeCustom()
        {
            var s = new CustomBinaryFormatter();
            var message = new CustomTypicalMessage()
            {
                StringProp = "hello",
                GuidProp = Guid.NewGuid(),
                IntProp = 123,
                DateProp = DateTime.UtcNow,
            };
            s.Register<CustomTypicalMessage>(0);

            var stream = new MemoryStream();
            s.Serialize(stream, message);
            var bytes = stream.ToArray();
            RunTest("Greg Young Custom Serializer", () =>
            {
                var stream2 = new MemoryStream();
                s.Serialize(stream2, message);
            },
                () =>
                {
                    stream.Position = 0;
                    s.Deserialize(stream);
                }, bytes.Length);

        }
    }
}
