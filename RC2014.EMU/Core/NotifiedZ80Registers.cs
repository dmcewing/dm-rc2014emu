using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Core
{
    public class NotifiedZ80Registers : NotifiedMainRegisters, IZ80Registers, INotifyPropertyChanged
    {
        public NotifiedZ80Registers()
        {
            Alternate = new NotifiedMainRegisters();
        }

        private IMainZ80Registers _Alternate;

        public IMainZ80Registers Alternate
        {
            get
            {
                return _Alternate;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Alternate");

                _Alternate = value;
            }
        }


        private short iX;
        private short iY;
        private ushort pC;
        private short sP;
        private short iR;
        private Bit iFF1;
        private Bit iFF2;
        
        public short IX { get => iX; set { iX = value; NotifyPropertyChanged(); } }
        public short IY { get => iY; set { iY = value; NotifyPropertyChanged(); } }
        public ushort PC { get => pC; set { pC = value; NotifyPropertyChanged(); } }
        public short SP { get => sP; set { sP = value; NotifyPropertyChanged(); } }
        public short IR { get => iR; set { iR = value; NotifyPropertyChanged(); } }
        public Bit IFF1 { get => iFF1; set { iFF1 = value; NotifyPropertyChanged(); } }
        public Bit IFF2 { get => iFF2; set { iFF2 = value; NotifyPropertyChanged(); } }

        
        public byte IXH
        {
            get
            {
                return IX.GetHighByte();
            }
            set
            {
                IX = IX.SetHighByte(value);
            }
        }

        public byte IXL
        {
            get
            {
                return IX.GetLowByte();
            }
            set
            {
                IX = IX.SetLowByte(value);
            }
        }

        public byte IYH
        {
            get
            {
                return IY.GetHighByte();
            }
            set
            {
                IY = IY.SetHighByte(value);
            }
        }

        public byte IYL
        {
            get
            {
                return IY.GetLowByte();
            }
            set
            {
                IY = IY.SetLowByte(value);
            }
        }

        public byte I
        {
            get
            {
                return IR.GetHighByte();
            }
            set
            {
                IR = IR.SetHighByte(value);
            }
        }

        public byte R
        {
            get
            {
                return IR.GetLowByte();
            }
            set
            {
                IR = IR.SetLowByte(value);
            }
        }
    }
}
