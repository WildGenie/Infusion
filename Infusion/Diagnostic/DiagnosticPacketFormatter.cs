﻿using System;
using System.Text;
using Infusion.Packets;

namespace Infusion.Diagnostic
{
    internal sealed class DiagnosticPacketFormatter
    {
        private const int MaxColumns = 16;
        private readonly StringBuilder builder = new StringBuilder();
        private readonly string header;
        private readonly PacketDefinitionRegistry packetRegistry;
        private int columns;
        private bool requiresHeader = true;
        private bool needsNewLine = false;

        public DiagnosticPacketFormatter(string header, PacketDefinitionRegistry packetRegistry)
        {
            this.header = header;
            this.packetRegistry = packetRegistry;
        }

        public void Header()
        {
            if (requiresHeader)
            {
                builder.AppendFormat($"{DateTime.UtcNow:hh:mm:ss.fff} >>>> {header}");
                builder.AppendLine();
                requiresHeader = false;
            }
        }

        public void DumpPacket(Packet packet)
        {
            if (needsNewLine)
            {
                builder.AppendLine();
                needsNewLine = false;
            }

            builder.AppendFormat(
                $"{DateTime.UtcNow:hh:mm:ss.fff} >>>> {header}: RawPacket {packetRegistry.Find(packet.Id).Name}, length = {packet.Length}");
            builder.AppendLine();

            bool justAppendedNewLine = true;
            for (var i = 0; i < packet.Length; i++)
            {
                justAppendedNewLine = false;
                builder.AppendFormat("0x{0:X2}, ", packet.Payload[i]);
                if ((i + 1)%MaxColumns == 0)
                {
                    builder.AppendLine();
                    justAppendedNewLine = true;
                }
            }

            if (!justAppendedNewLine)
            {
                builder.AppendLine();
            }

            columns = 0;
            requiresHeader = true;
        }

        public void AddByte(byte value)
        {
            Header();

            needsNewLine = true;

            builder.AppendFormat("0x{0:X2}, ", value);
            columns++;

            if (columns + 1 > MaxColumns)
            {
                columns = 0;
                builder.AppendLine();
                needsNewLine = false;
            }
        }

        public string Flush()
        {
            if (needsNewLine)
            {
                builder.AppendLine();
            }

            var result = builder.ToString();

            builder.Clear();
            needsNewLine = false;
            requiresHeader = true;

            return result;
        }
    }
}