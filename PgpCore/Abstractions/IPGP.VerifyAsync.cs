﻿using PgpCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PgpCore.Abstractions
{
    public interface IVerifyAsync : IDisposable
    {
        Task<bool> VerifyAsync(FileInfo inputFile, FileInfo outputFile = null, bool throwIfEncrypted = false);
        Task<bool> VerifyAsync(Stream inputStream, Stream outputStream = null, bool throwIfEncrypted = false);
        Task<bool> VerifyAsync(string input, string output = null, bool throwIfEncrypted = false);
        Task<bool> VerifyClearAsync(FileInfo inputFile, FileInfo outputFile = null);
        Task<bool> VerifyClearAsync(Stream inputStream, Stream outputStream = null);
        Task<bool> VerifyClearAsync(string input, string output = null);

        Task<bool> VerifyFileAsync(FileInfo inputFile, bool throwIfEncrypted = false);
        Task<bool> VerifyStreamAsync(Stream inputStream, bool throwIfEncrypted = false);
        Task<bool> VerifyArmoredStringAsync(string input, bool throwIfEncrypted = false);
        Task<bool> VerifyClearFileAsync(FileInfo inputFile);
        Task<bool> VerifyClearStreamAsync(Stream inputStream);
        Task<bool> VerifyClearArmoredStringAsync(string input);
        Task<VerificationResult> VerifyAndReadClearFileAsync(FileInfo inputFile);
        Task<VerificationResult> VerifyAndReadClearStreamAsync(Stream inputStream);
        Task<VerificationResult> VerifyAndReadClearArmoredStringAsync(string input);
        Task<VerificationResult> VerifyAndReadSignedFileAsync(FileInfo inputFile, bool throwIfEncrypted = false);
        Task<VerificationResult> VerifyAndReadSignedStreamAsync(Stream inputStream, bool throwIfEncrypted = false);
        Task<VerificationResult> VerifyAndReadSignedArmoredStringAsync(string input, bool throwIfEncrypted = false);
    }
}
