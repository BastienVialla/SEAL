﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Research.SEAL.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.Research.SEAL
{
    /// <summary>
    /// Generates matching secret key and public key. An existing KeyGenerator can 
    /// also at any time be used to generate relinearization keys and Galois keys. 
    /// Constructing a KeyGenerator requires only a SEALContext.
    /// </summary>
    /// <see cref="EncryptionParameters">see EncryptionParameters for more details on encryption parameters.</see>
    /// <see cref="SEAL.SecretKey">see SecretKey for more details on secret key.</see>
    /// <see cref="SEAL.PublicKey">see PublicKey for more details on public key.</see>
    /// <see cref="SEAL.RelinKeys">see RelinKeys for more details on relinearization keys.</see>
    /// <see cref="SEAL.GaloisKeys">see GaloisKeys for more details on Galois keys.</see>
    public class KeyGenerator : NativeObject
    {
        /// <summary>
        /// Creates a KeyGenerator initialized with the specified SEALContext.
        /// </summary>
        /// 
        /// <remarks>
        /// Creates a KeyGenerator initialized with the specified <see cref="SEALContext" />.
        /// Dynamically allocated member variables are allocated from the global memory pool.
        /// </remarks>
        /// <param name="context">The SEALContext</param>
        /// <exception cref="ArgumentException">if encryption parameters are not 
        /// valid</exception>
        /// <exception cref="ArgumentNullException">if context is null</exception>
        public KeyGenerator(SEALContext context)
        {
            if (null == context)
                throw new ArgumentNullException(nameof(context));

            NativeMethods.KeyGenerator_Create(context.NativePtr, out IntPtr ptr);
            NativePtr = ptr;
        }

        /// <summary>
        /// Creates an KeyGenerator instance initialized with the specified
        /// SEALContext and specified previously secret key. This can e.g. be used 
        /// to increase the number of relinearization keys from what had earlier 
        /// been generated, or to generate Galois keys in case they had not been 
        /// generated earlier.
        /// </summary>
        /// <param name="context">The SEALContext</param>
        /// <param name="secretKey">A previously generated secret key</param>
        /// <exception cref="ArgumentNullException">if either context or secretKey are null</exception>
        /// <exception cref="ArgumentException">if encryption parameters are not valid</exception>
        /// <exception cref="ArgumentException">if secret_key or public_key is not valid
        /// for encryption parameters</exception>
        public KeyGenerator(SEALContext context, SecretKey secretKey)
        {
            if (null == context)
                throw new ArgumentNullException(nameof(context));
            if (null == secretKey)
                throw new ArgumentNullException(nameof(secretKey));

            NativeMethods.KeyGenerator_Create(context.NativePtr, secretKey.NativePtr, out IntPtr ptr);
            NativePtr = ptr;
        }

        /// <summary>
        /// Creates an KeyGenerator instance initialized with the specified 
        /// SEALContext and specified previously secret and public keys. This can 
        /// e.g. be used to increase the number of relinearization keys from what 
        /// had earlier been generated, or to generate Galois keys in case they 
        /// had not been generated earlier.
        /// </summary>
        /// <param name="context">The SEALContext</param>
        /// <param name="secretKey">A previously generated secret key</param>
        /// <param name="publicKey">A previously generated public key</param>
        /// <exception cref="ArgumentNullException">if either context, secretKey or publicKey are null</exception>
        /// <exception cref="ArgumentException">if encryption parameters are not valid</exception>
        /// <exception cref="ArgumentException">if secret_key or public_key is not valid 
        /// for encryption parameters</exception>
        public KeyGenerator(SEALContext context, SecretKey secretKey, PublicKey publicKey)
        {
            if (null == context)
                throw new ArgumentNullException(nameof(context));
            if (null == secretKey)
                throw new ArgumentNullException(nameof(secretKey));
            if (null == publicKey)
                throw new ArgumentNullException(nameof(publicKey));

            NativeMethods.KeyGenerator_Create(context.NativePtr, secretKey.NativePtr, publicKey.NativePtr, out IntPtr ptr);
            NativePtr = ptr;
        }

        /// <summary>
        /// Returns a copy of the public key.
        /// </summary>
        public PublicKey PublicKey
        {
            get
            {
                NativeMethods.KeyGenerator_PublicKey(NativePtr, out IntPtr pubKeyPtr);
                PublicKey pubKey = new PublicKey(pubKeyPtr, owned: false);
                return pubKey;
            }
        }

        /// <summary>
        /// Returns a copy of the secret key.
        /// </summary>
        public SecretKey SecretKey
        {
            get
            {
                NativeMethods.KeyGenerator_SecretKey(NativePtr, out IntPtr secretKeyPtr);
                SecretKey secretKey = new SecretKey(secretKeyPtr, owned: false);
                return secretKey;
            }
        }

        /// <summary>
        /// Generates and returns the specified number of relinearization keys.
        /// </summary>
        /// <param name="decompositionBitCount">The decomposition bit count</param>
        /// <param name="count">The number of relinearization keys to generate</param>
        /// <exception cref="ArgumentException">if decompositionBitCount is not within [1, 60]</exception>
        /// <exception cref="ArgumentException">if count is negative</exception>
        public RelinKeys RelinKeys(int decompositionBitCount, int count = 1)
        {
            if (decompositionBitCount < 1 || decompositionBitCount > 60)
                throw new ArgumentException("decompositionBitCount should be within [1, 60]");
            if (count < 0)
                throw new ArgumentException("count is negative");

            NativeMethods.KeyGenerator_RelinKeys(NativePtr, decompositionBitCount, count, out IntPtr relinKeysPtr);
            return new RelinKeys(relinKeysPtr);
        }

        /// <summary>
        /// Generates Galois keys.
        /// </summary>
        /// 
        /// <remarks>
        /// Generates Galois keys. This function creates logarithmically many (in degree of the
        /// polynomial modulus) Galois keys that is sufficient to apply any Galois automorphism
        /// (e.g. rotations) on encrypted data. Most users will want to use this overload of
        /// the function.
        /// </remarks>
        /// <param name="decompositionBitCount">The decomposition bit count</param>
        /// <exception cref="ArgumentException">if decompositionBitCount is not
        /// within [1, 60]</exception>
        public GaloisKeys GaloisKeys(int decompositionBitCount)
        {
            if (decompositionBitCount < 1 || decompositionBitCount > 60)
                throw new ArgumentException("decompositionBitCount should be within [1, 60]");

            NativeMethods.KeyGenerator_GaloisKeys(NativePtr, decompositionBitCount, out IntPtr galoisKeysPtr);
            return new GaloisKeys(galoisKeysPtr);
        }

        /// <summary>
        /// Generates Galois keys.
        /// </summary>
        /// 
        /// <remarks>
        /// Generates Galois keys. This function creates specific Galois keys that can be used to
        /// apply specific Galois automorphisms on encrypted data. The user needs to give as
        /// input a vector of Galois elements corresponding to the keys that are to be created.
        /// 
        /// The Galois elements are odd integers in the interval [1, M-1], where M = 2*N, and
        /// N = degree(PolyModulus). Used with batching, a Galois element 3^i % M corresponds 
        /// to a cyclic row rotation i steps to the left, and a Galois element 3^(N/2-i) % M 
        /// corresponds to a cyclic row rotation i steps to the right. The Galois element M-1 
        /// corresponds to a column rotation (row swap). In the polynomial view (not batching), 
        /// a Galois automorphism by a Galois element p changes Enc(plain(x)) to Enc(plain(x^p)).
        /// </remarks>
        /// <param name="decompositionBitCount">The decomposition bit count</param>
        /// <param name="galoisElts">The Galois elements for which to generate keys</param>
        /// <exception cref="ArgumentException">if decompositionBitCount is not
        /// within [1, 60]</exception>
        /// <exception cref="ArgumentException">if the Galois elements are not 
        /// valid</exception>
        public GaloisKeys GaloisKeys(int decompositionBitCount,
            IEnumerable<ulong> galoisElts)
        {
            if (null == galoisElts)
                throw new ArgumentNullException(nameof(galoisElts));

            ulong[] galoisEltsArr = galoisElts.ToArray();
            NativeMethods.KeyGenerator_GaloisKeys(NativePtr, decompositionBitCount, galoisEltsArr.Length, galoisEltsArr, out IntPtr galoisKeysPtr);
            return new GaloisKeys(galoisKeysPtr);
        }

        /// <summary>
        /// Generates and returns Galois keys. This function creates specific Galois 
        /// keys that can be used to apply specific Galois automorphisms on encrypted 
        /// data. The user needs to give as input a vector of desired Galois rotation 
        /// step counts, where negative step counts correspond to rotations to the 
        /// right and positive step counts correspond to rotations to the left. 
        /// A step count of zero can be used to indicate a column rotation in the BFV 
        /// scheme complex conjugation in the CKKS scheme.
        /// </summary>
        /// <param name="decompositionBitCount">The decomposition bit count</param>
        /// <param name="steps">The rotation step counts for which to generate keys</param>
        /// <exception cref="ArgumentNullException">if steps is null</exception>
        /// <exception cref="InvalidOperationException">if the encryption parameters do not support batching
        /// and scheme is SchemeType.BFV</exception>
        /// <exception cref="ArgumentException">if decompositionBitCount is not within [1, 60]</exception>
        /// <exception cref="ArgumentException">if the step counts are not valid</exception>
        public GaloisKeys GaloisKeys(int decompositionBitCount,
            IEnumerable<int> steps)
        {
            if (null == steps)
                throw new ArgumentNullException(nameof(steps));

            try
            {
                int[] stepsArr = steps.ToArray();
                NativeMethods.KeyGenerator_GaloisKeys(NativePtr, decompositionBitCount, stepsArr.Length, stepsArr, out IntPtr galoisKeys);
                return new GaloisKeys(galoisKeys);
            }
            catch (COMException ex)
            {
                if ((uint)ex.HResult == NativeMethods.Errors.HRInvalidOperation)
                    throw new InvalidOperationException("Encryption parameters do not support batching and scheme is SchemeType.BFV", ex);
                throw;
            }
        }

        /// <summary>
        /// Destroy native object.
        /// </summary>
        protected override void DestroyNativeObject()
        {
            NativeMethods.KeyGenerator_Destroy(NativePtr);
        }
    }
}
