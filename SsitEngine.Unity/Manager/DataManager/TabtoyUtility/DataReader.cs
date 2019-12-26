using System;
using System.IO;
using System.Text;

namespace Tabtoy
{
    internal enum FieldType
    {
        None = 0,
        Int32 = 1,
        Int64 = 2,
        UInt32 = 3,
        UInt64 = 4,
        Float = 5,
        String = 6,
        Bool = 7,
        Enum = 8,
        Struct = 9
    }

    public delegate void DeserializeHandler<T>( T ins, DataReader reader );

    public class DataReader
    {
        private const int CombineFileVersion = 2;

        private static readonly UTF8Encoding encoding = new UTF8Encoding();
        private readonly long _boundPos = -1;

        public DataReader( Stream stream, long boundpos )
        {
            GetBinaryReader = new BinaryReader(stream);
            _boundPos = boundpos;
        }

        public DataReader( Stream stream )
        {
            GetBinaryReader = new BinaryReader(stream);
            _boundPos = stream.Length;
        }

        public DataReader( DataReader reader, long boundpos )
        {
            GetBinaryReader = reader.GetBinaryReader;
            _boundPos = boundpos;
        }

        public BinaryReader GetBinaryReader { get; }

        private void ConsumeData( int size )
        {
            if (!IsDataEnough(size))
            {
                throw new Exception("Out of struct bound");
            }
        }

        private bool IsDataEnough( int size )
        {
            return GetBinaryReader.BaseStream.Position + size <= _boundPos;
        }

        public bool ReadHeader()
        {
            var tag = ReadString();
            if (tag != "TABTOY")
            {
                return false;
            }

            var ver = ReadInt32();
            if (ver != CombineFileVersion)
            {
                return false;
            }

            return true;
        }

        public int ReadTag()
        {
            if (IsDataEnough(sizeof(int)))
            {
                return ReadInt32();
            }

            return -1;
        }

        public int ReadInt32()
        {
            ConsumeData(sizeof(int));

            return GetBinaryReader.ReadInt32();
        }

        public long ReadInt64()
        {
            ConsumeData(sizeof(long));

            return GetBinaryReader.ReadInt64();
        }

        public uint ReadUInt32()
        {
            ConsumeData(sizeof(uint));

            return GetBinaryReader.ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            ConsumeData(sizeof(ulong));

            return GetBinaryReader.ReadUInt64();
        }

        public float ReadFloat()
        {
            ConsumeData(sizeof(float));

            return GetBinaryReader.ReadSingle();
        }

        public bool ReadBool()
        {
            ConsumeData(sizeof(bool));

            return GetBinaryReader.ReadBoolean();
        }

        public string ReadString()
        {
            var len = ReadInt32();

            ConsumeData(sizeof(byte) * len);

            return encoding.GetString(GetBinaryReader.ReadBytes(len));
        }

        public T ReadEnum<T>()
        {
            return (T) Enum.ToObject(typeof(T), ReadInt32());
        }

        public DataReader ReadStruct()
        {
            var bound = GetBinaryReader.ReadInt32();
            return new DataReader(this, GetBinaryReader.BaseStream.Position + bound);
        }

        public T ReadStruct<T>( DeserializeHandler<T> handler ) where T : class
        {
            var bound = GetBinaryReader.ReadInt32();

            var element = Activator.CreateInstance<T>();

            handler(element, new DataReader(this, GetBinaryReader.BaseStream.Position + bound));

            return element;
        }
    }
}