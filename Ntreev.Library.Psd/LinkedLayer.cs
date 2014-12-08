﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd
{
    class LinkedLayer : ILinkedLayer
    {
        private readonly Guid id;
        private readonly string fileName;
        private PsdDocument document;
        private DescriptorStructure descriptor;

        public LinkedLayer(PSDReader reader)
        {
            long length = reader.ReadInt64();
            long position = reader.Position;

            string type = reader.ReadKey();
            int version = reader.ReadInt32();

            this.Validate(type, version);
            this.id = new Guid(reader.ReadPascalString(1));
            this.fileName = reader.ReadUnicodeString2();

            string fileType = reader.ReadKey();
            string fileCreator = reader.ReadKey();

            this.ReadDocument(reader);

            reader.Position = position + length;
            if (reader.Position % 2 != 0)
                reader.Position++;

            reader.Position += ((reader.Position - position) % 4);
        }

        public PsdDocument Document
        {
            get { return this.document; }
        }

        public Guid ID
        {
            get { return this.id; }
        }

        public string FileName
        {
            get { return this.fileName; }
        }

        public IProperties Properties
        {
            get { return this.descriptor; }
        }

        public virtual bool IsEmbedded
        {
            get { return false; }
        }

        protected virtual void Validate(string type, int version)
        {
            if (type != "liFD" || version < 2)
                throw new Exception("Invalid PSD file");
        }

        protected virtual string Path
        {
            get { return string.Empty; }
        }

        private void ReadDocument(PSDReader reader)
        {
            long length = reader.ReadInt64();
            long position = reader.Position;

            bool fod = reader.ReadBoolean();
            if (fod == true)
            {
                var version = reader.ReadInt32();
                this.descriptor = new DescriptorStructure(reader);
            }

            if (length > 0)
            {
                byte[] bytes = reader.ReadBytes((int)length);

                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    if (this.IsDocument(bytes) == true)
                    {
                        PsdDocument psb = new PsdDocument(this.fileName);
                        psb.Read(stream);
                        this.document = psb;
                    }
                    else
                    {
                        //throw new NotSupportedException();
                    }
                }
            }
            else
            {
                this.document = reader.Resolver.GetDocument(this.fileName);
            }
        }

        private bool IsDocument(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            using (PSDReader reader = new PSDReader(stream, null))
            {
                string key = reader.ReadKey();
                Console.WriteLine(key);
                return key == "8BPS";
            }
        }
    }
}
