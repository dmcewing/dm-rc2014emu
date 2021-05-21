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
    public class NotifiedMainRegisters : IMainZ80Registers, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private short aF;
        private short bC;
        private short dE;
        private short hL;

        public short BC { get => bC; set { bC = value; NotifyPropertyChanged(); } }
        public short AF { get => aF; set { aF = value; NotifyPropertyChanged(); } }
        public short DE { get => dE; set { dE = value; NotifyPropertyChanged(); } }
        public short HL { get => hL; set { hL = value; NotifyPropertyChanged(); } }

        public byte A
        {
            get
            {
                return AF.GetHighByte();
            }
            set
            {
                AF = AF.SetHighByte(value);
            }
        }

        public byte F
        {
            get
            {
                return AF.GetLowByte();
            }
            set
            {
                AF = AF.SetLowByte(value);
            }
        }

        public byte B
        {
            get
            {
                return BC.GetHighByte();
            }
            set
            {
                BC = BC.SetHighByte(value);
            }
        }

        public byte C
        {
            get
            {
                return BC.GetLowByte();
            }
            set
            {
                BC = BC.SetLowByte(value);
            }
        }

        public byte D
        {
            get
            {
                return DE.GetHighByte();
            }
            set
            {
                DE = DE.SetHighByte(value);
            }
        }

        public byte E
        {
            get
            {
                return DE.GetLowByte();
            }
            set
            {
                DE = DE.SetLowByte(value);
            }
        }

        public byte H
        {
            get
            {
                return HL.GetHighByte();
            }
            set
            {
                HL = HL.SetHighByte(value);
            }
        }

        public byte L
        {
            get
            {
                return HL.GetLowByte();
            }
            set
            {
                HL = HL.SetLowByte(value);
            }
        }

        public Bit CF
        {
            get
            {
                return F.GetBit(0);
            }
            set
            {
                F = F.WithBit(0, value);
            }
        }

        public Bit NF
        {
            get
            {
                return F.GetBit(1);
            }
            set
            {
                F = F.WithBit(1, value);
            }
        }

        public Bit PF
        {
            get
            {
                return F.GetBit(2);
            }
            set
            {
                F = F.WithBit(2, value);
            }
        }

        public Bit Flag3
        {
            get
            {
                return F.GetBit(3);
            }
            set
            {
                F = F.WithBit(3, value);
            }
        }

        public Bit HF
        {
            get
            {
                return F.GetBit(4);
            }
            set
            {
                F = F.WithBit(4, value);
            }
        }

        public Bit Flag5
        {
            get
            {
                return F.GetBit(5);
            }
            set
            {
                F = F.WithBit(5, value);
            }
        }

        public Bit ZF
        {
            get
            {
                return F.GetBit(6);
            }
            set
            {
                F = F.WithBit(6, value);
            }
        }

        public Bit SF
        {
            get
            {
                return F.GetBit(7);
            }
            set
            {
                F = F.WithBit(7, value);
            }
        }

    }
}
