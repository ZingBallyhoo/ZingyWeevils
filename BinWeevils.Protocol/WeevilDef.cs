using System;
using System.IO;

namespace BinWeevils.Protocol
{
    public class WeevilDef
    {
        public enum BodyType : byte
        {
            Spheroid = 1,
            Cone = 2,
            ConeNarrowInv = 3,
            Cuboid = 4,
            Count,
        }
        
        public enum HeadType : byte
        {
            Spheroid = 1,
            Cone = 2,
            ConeInv = 3,
            Cuboid = 4,
            Count,
        }

        public enum EyeType : byte
        {
            MiddleTogether = 1,
            MiddleApart = 2,
            HighTogether = 3,
            TopTogether = 4,
            TopApart = 5,
            MiddleSide = 6,
            Count,
        }
        
        public enum AntennaType : byte
        {
            None = 0,
            
            SingleSmall = 1,
            SingleMedium = 2,
            SingleLarge = 3,
            
            DoubleSmall = 4,
            DoubleMedium = 5,
            DoubleLarge = 6,
            
            TripleSmall = 7,
            TripleMedium = 8,
            TripleLarge = 9,
            
            SuperOriginal = 10,
            SuperPurple = 11,
            SuperRedWhite = 12,
            SuperPurpleYellowBlue = 13,
            SuperHalloween = 14,
            SuperFire = 15,
            SuperIce = 16,
            Count,
            CountStandard = SuperOriginal,
        }

        public enum LegType : byte
        {
            Normal = 0,
            Stripy = 1,
            SuperLegsSummerFair = 2,
            SuperLegsOriginal = 3,
            SuperLegsPurpleYellowBlue = 4,
            SuperLegsHalloween = 5,
            Count,
        }
        
        public HeadType m_headType;
        public byte m_headColorIdx;
        
        public BodyType m_bodyType;
        public byte m_bodyColorIdx;
        
        public EyeType m_eyeType;
        public byte m_eyeColorIdx;
        public bool m_lids;
        
        public AntennaType m_antennaType;
        public byte m_antennaColorIdx;
        
        public byte m_legColorIdx;
        public LegType m_legType;

        public const int COLOR_COUNT = 52;
        public const int EYE_COLOR_COUNT = 58;
        
        // todo: there are actually 23 but the def changer only allows up to 22
        public const int LEGACY_COLOR_COUNT = 22;
        public const int LEGACY_EYE_COLOR_COUNT = 12;

        public const ulong DEFAULT = 101101406100171700; // pea/cabbage
        public const ulong ZINGY = 102311611105070700;
        public const ulong DEFINITELY_SCRIBBLES = 201421625110171700;

        public WeevilDef(ulong num) : this($"{num}")
        { }
        
        public WeevilDef(string str) : this(str.AsSpan())
        { }

        public WeevilDef(ReadOnlySpan<char> span)
        {
            m_headType = (HeadType)byte.Parse(span.Slice(0, 1));
            m_headColorIdx = byte.Parse(span.Slice(1, 2));
            
            m_bodyType = (BodyType)byte.Parse(span.Slice(3, 1));
            m_bodyColorIdx = byte.Parse(span.Slice(4, 2));
            
            m_eyeType = (EyeType)byte.Parse(span.Slice(6, 1));
            m_eyeColorIdx = byte.Parse(span.Slice(7, 2));
            m_lids = byte.Parse(span.Slice(9, 1)) != 0;
            
            m_antennaType = (AntennaType)byte.Parse(span.Slice(10, 2));
            m_antennaColorIdx = byte.Parse(span.Slice(12, 2));
            
            m_legColorIdx = byte.Parse(span.Slice(14, 2));
            switch (span.Length)
            {
                case 16:
                {
                    m_legType = LegType.Normal;
                    break;
                }
                case 17:
                case 18:
                {
                    m_legType = (LegType)byte.Parse(span.Slice(16, span.Length-16));
                    break;
                }
                default:
                {
                    throw new InvalidDataException($"weevildef with wrong string length: \"{span}\" ({span.Length})");
                }
            }
        }

        public string AsString()
        {
            return $"{(byte)m_headType:D1}{m_headColorIdx:D2}" +
                   $"{(byte)m_bodyType:D1}{m_bodyColorIdx:D2}" +
                   $"{(byte)m_eyeType:D1}{m_eyeColorIdx:D2}{(m_lids ? '1': '0')}" +
                   $"{(byte)m_antennaType:D2}{m_antennaColorIdx:D2}" +
                   $"{m_legColorIdx:D2}{(byte)m_legType:D2}";
        }
        
        public ulong AsNumber()
        {
            return ulong.Parse(AsString());
        }

        private bool ValidateEnums =>
            Enum.IsDefined(m_headType) && Enum.IsDefined(m_bodyType) && Enum.IsDefined(m_eyeType) &&
            Enum.IsDefined(m_antennaType) && Enum.IsDefined(m_legType);
        
        private bool ValidateColors =>
            m_headColorIdx < COLOR_COUNT && 
            m_bodyColorIdx < COLOR_COUNT && 
            m_antennaColorIdx < COLOR_COUNT && 
            m_legColorIdx < COLOR_COUNT &&
            m_eyeColorIdx < EYE_COLOR_COUNT;
        
        private bool ValidateLegacyColors =>
            m_headColorIdx < LEGACY_COLOR_COUNT && 
            m_bodyColorIdx < LEGACY_COLOR_COUNT && 
            m_antennaColorIdx < LEGACY_COLOR_COUNT && 
            m_legColorIdx < LEGACY_COLOR_COUNT &&
            m_eyeColorIdx < LEGACY_EYE_COLOR_COUNT;

        public bool Validate()
        {
            return ValidateEnums && ValidateColors;
        }
        
        public bool ValidateLegacy()
        {
            return ValidateEnums && 
                   ValidateLegacyColors &&
                   m_legType == LegType.Normal &&
                   m_antennaType <= AntennaType.SuperOriginal;
        }

        public bool HasSuperAntenna()
        {
            return m_antennaType == AntennaType.SuperOriginal ||
                   m_antennaType == AntennaType.SuperPurple ||
                   m_antennaType == AntennaType.SuperRedWhite ||
                   m_antennaType == AntennaType.SuperPurpleYellowBlue ||
                   m_antennaType == AntennaType.SuperHalloween ||
                   m_antennaType == AntennaType.SuperFire ||
                   m_antennaType == AntennaType.SuperIce;
        }
    }
}