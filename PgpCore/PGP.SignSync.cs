﻿using Org.BouncyCastle.Bcpg;
using PgpCore.Abstractions;
using PgpCore.Extensions;
using PgpCore.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PgpCore
{
    public partial class PGP : ISignSync
    {
        #region Sign

        /// <summary>
        /// Sign the file pointed to by unencryptedFileInfo
        /// </summary>
        /// <param name="inputFile">Plain data file to be signed</param>
        /// <param name="outputFile">Output PGP signed file</param>
        /// <param name="armor">True, means a binary data representation as an ASCII-only text. Otherwise, false</param>
        /// <param name="name">Name of signed file in message, defaults to the input file name</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public void Sign(
            FileInfo inputFile,
            FileInfo outputFile,
            bool armor = true,
            string name = null,
            IDictionary<string, string> headers = null)
        {
            if (inputFile == null)
                throw new ArgumentException("InputFile");
            if (outputFile == null)
                throw new ArgumentException("OutputFile");
            if (EncryptionKeys == null)
                throw new ArgumentException("EncryptionKeys");
            if (string.IsNullOrEmpty(name))
                name = Path.GetFileName(inputFile.Name);
            if (headers == null)
                headers = new Dictionary<string, string>();

            if (!inputFile.Exists)
                throw new FileNotFoundException($"Input file [{inputFile.FullName}] does not exist.");

            using (Stream outputStream = outputFile.OpenWrite())
            {
                if (armor)
                {
                    using (ArmoredOutputStream armoredOutputStream = new ArmoredOutputStream(outputStream, headers))
                    {
                        OutputSigned(inputFile, armoredOutputStream, name);
                    }
                }
                else
                    OutputSigned(inputFile, outputStream, name);
            }
        }

        /// <summary>
        /// Sign the stream pointed to by unencryptedFileInfo and
        /// </summary>
        /// <param name="inputStream">Plain data stream to be signed</param>
        /// <param name="outputStream">Output PGP signed stream</param>
        /// <param name="armor">True, means a binary data representation as an ASCII-only text. Otherwise, false</param>
        /// <param name="name">Name of signed file in message, defaults to the stream name if the stream is a FileStream, otherwise to "name"</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public void Sign(
            Stream inputStream,
            Stream outputStream,
            bool armor = true,
            string name = null,
            IDictionary<string, string> headers = null)
        {
            if (inputStream == null)
                throw new ArgumentException("InputStream");
            if (outputStream == null)
                throw new ArgumentException("OutputStream");
            if (EncryptionKeys == null)
                throw new ArgumentException("EncryptionKeys");
            if (inputStream.Position != 0)
                throw new ArgumentException("inputStream should be at start of stream");
            if (string.IsNullOrEmpty(name) && inputStream is FileStream fileStream)
                Path.GetFileName(fileStream.Name);
            else if (string.IsNullOrEmpty(name))
                name = DefaultFileName;
            if (headers == null)
                headers = new Dictionary<string, string>();

            if (armor)
            {
                using (ArmoredOutputStream armoredOutputStream = new ArmoredOutputStream(outputStream, headers))
                {
                    OutputSigned(inputStream, armoredOutputStream, name);
                }
            }
            else
                OutputSigned(inputStream, outputStream, name);
        }

        /// <summary>
        /// Sign the string
        /// </summary>
        /// <param name="input">Plain string to be signed</param>
        /// <param name="name">Name of signed file in message, defaults to "name"</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public string Sign(
            string input,
            bool armor = true,
            string name = null,
            IDictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(name))
                name = DefaultFileName;
            if (headers == null)
                headers = new Dictionary<string, string>();

            using (Stream inputStream = input.GetStream())
            using (Stream outputStream = new MemoryStream())
            {
                Sign(inputStream, outputStream, armor, name, headers);
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream.GetString();
            }
        }

        public void SignFile(FileInfo inputFile, FileInfo outputFile, bool armor = true, string name = null, IDictionary<string, string> headers = null) => Sign(inputFile, outputFile, armor, name, headers);

        public void SignStream(Stream inputStream, Stream outputStream, bool armor = true, string name = null, IDictionary<string, string> headers = null) => Sign(inputStream, outputStream, armor, name, headers);

        public string SignArmoredString(string input, bool armor = true, string name = null, IDictionary<string, string> headers = null) => Sign(input, armor, name, headers);

        #endregion Sign

        #region ClearSign

        /// <summary>
        /// Clear sign the file pointed to by unencryptedFileInfo
        /// </summary>
        /// <param name="inputFile">Plain data file to be signed</param>
        /// <param name="outputFile">Output PGP signed file</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public void ClearSign(
            FileInfo inputFile,
            FileInfo outputFile,
            IDictionary<string, string> headers = null)
        {
            if (inputFile == null)
                throw new ArgumentException("InputFile");
            if (outputFile == null)
                throw new ArgumentException("OutputFile");
            if (EncryptionKeys == null)
                throw new ArgumentException("EncryptionKeys");
            if (headers == null)
                headers = new Dictionary<string, string>();

            if (!inputFile.Exists)
                throw new FileNotFoundException($"Input file [{inputFile.Name}] does not exist.");

            using (Stream outputStream = outputFile.OpenWrite())
            {
                OutputClearSigned(inputFile, outputStream, headers);
            }
        }

        /// <summary>
        /// Clear sign the provided stream
        /// </summary>
        /// <param name="inputStream">Plain data stream to be signed</param>
        /// <param name="outputStream">Output PGP signed stream</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public void ClearSign(
            Stream inputStream,
            Stream outputStream,
            IDictionary<string, string> headers = null)
        {
            if (inputStream == null)
                throw new ArgumentException("InputStream");
            if (outputStream == null)
                throw new ArgumentException("OutputStream");
            if (EncryptionKeys == null)
                throw new ArgumentException("EncryptionKeys");
            if (inputStream.Position != 0)
                throw new ArgumentException("inputStream should be at start of stream");
            if (headers == null)
                headers = new Dictionary<string, string>();

            OutputClearSigned(inputStream, outputStream, headers);
        }

        /// <summary>
        /// Clear sign the provided string
        /// </summary>
        /// <param name="input">Plain string to be signed</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public string ClearSign(
            string input,
            IDictionary<string, string> headers = null)
        {
            if (headers == null)
                headers = new Dictionary<string, string>();

            using (Stream inputStream = input.GetStream())
            using (Stream outputStream = new MemoryStream())
            {
                ClearSign(inputStream, outputStream, headers);
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream.GetString();
            }
        }

        public void ClearSignFile(FileInfo inputFile, FileInfo outputFile, IDictionary<string, string> headers = null) => ClearSign(inputFile, outputFile, headers);

        public void ClearSignStream(Stream inputStream, Stream outputStream, IDictionary<string, string> headers = null) => ClearSign(inputStream, outputStream, headers);

        public string ClearSignArmoredString(string input, IDictionary<string, string> headers = null) => ClearSign(input, headers);

        #endregion ClearSign

        #region DetachSign

        /// <summary>
        /// Detach sign the file pointed to by unencryptedFileInfo
        /// </summary>
        /// <param name="inputFile">Plain data file to be signed</param>
        /// <param name="outputFile">Output PGP signature file</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public void DetachSign(
            FileInfo inputFile,
            FileInfo outputFile,
            IDictionary<string, string> headers = null)
        {
            if (inputFile == null)
                throw new ArgumentException("InputFile");
            if (outputFile == null)
                throw new ArgumentException("OutputFile");
            if (EncryptionKeys == null)
                throw new ArgumentException("EncryptionKeys");
            if (headers == null)
                headers = new Dictionary<string, string>();

            if (!inputFile.Exists)
                throw new FileNotFoundException($"Input file [{inputFile.Name}] does not exist.");

            using (Stream outputStream = outputFile.OpenWrite())
            {
                OutputDetachSigned(inputFile, outputStream, headers);
            }
        }

        /// <summary>
        /// Detach sign the provided stream
        /// </summary>
        /// <param name="inputStream">Plain data stream to be signed</param>
        /// <param name="outputStream">Output PGP signature stream</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public void DetachSign(
            Stream inputStream,
            Stream outputStream,
            IDictionary<string, string> headers = null)
        {
            if (inputStream == null)
                throw new ArgumentException("InputStream");
            if (outputStream == null)
                throw new ArgumentException("OutputStream");
            if (EncryptionKeys == null)
                throw new ArgumentException("EncryptionKeys");
            if (inputStream.Position != 0)
                throw new ArgumentException("inputStream should be at start of stream");
            if (headers == null)
                headers = new Dictionary<string, string>();

            OutputDetachSigned(inputStream, outputStream, headers);
        }

        /// <summary>
        /// Detach sign the provided string
        /// </summary>
        /// <param name="input">Plain string to be signed</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public string DetachSign(
            string input,
            IDictionary<string, string> headers = null)
        {
            if (headers == null)
                headers = new Dictionary<string, string>();

            using (Stream inputStream = input.GetStream())
            using (Stream outputStream = new MemoryStream())
            {
                DetachSign(inputStream, outputStream, headers);
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream.GetString();
            }
        }

        public void DetachSignFile(FileInfo inputFile, FileInfo outputFile, IDictionary<string, string> headers = null) => DetachSign(inputFile, outputFile, headers);

        public void DetachSignStream(Stream inputStream, Stream outputStream, IDictionary<string, string> headers = null) => DetachSign(inputStream, outputStream, headers);

        public string DetachSignArmoredString(string input, IDictionary<string, string> headers = null) => DetachSign(input, headers);

        #endregion DetachSign
    }
}
