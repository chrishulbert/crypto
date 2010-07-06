// Simple, thoroughly commented implementation of DES / Triple DES using C#
// Chris Hulbert - chris.hulbert@gmail.com - http://splinter.com.au/blog - http://github.com/chrishulbert/crypto
// Reference: http://orlingrabbe.com/des.htm
// To compile this, in the visual studio command prompt do: 'csc cs-des.cs'

using System;
using System.Collections.Generic;

class Program
{
  // S-box lookups transformed so you don't have to figure out rows and columns
  static byte[] s1 = { 14, 0,  4,  15, 13, 7,  1,  4,  2,  14, 15, 2,  11, 13, 8,  1,  3,  10, 10, 6,  6,  12, 12, 11, 5,  9,  9,  5,  0,  3,  7,  8,  4,  15, 1,  12, 14, 8,  8,  2,  13, 4,  6,  9,  2,  1,  11, 7,  15, 5,  12, 11, 9,  3,  7,  14, 3,  10, 10, 0,  5,  6,  0,  13, };
  static byte[] s2 = { 15, 3,  1,  13, 8,  4,  14, 7,  6,  15, 11, 2,  3,  8,  4,  14, 9,  12, 7,  0,  2,  1,  13, 10, 12, 6,  0,  9,  5,  11, 10, 5,  0,  13, 14, 8,  7,  10, 11, 1,  10, 3,  4,  15, 13, 4,  1,  2,  5,  11, 8,  6,  12, 7,  6,  12, 9,  0,  3,  5,  2,  14, 15, 9,  };
  static byte[] s3 = { 10, 13, 0,  7,  9,  0,  14, 9,  6,  3,  3,  4,  15, 6,  5,  10, 1,  2,  13, 8,  12, 5,  7,  14, 11, 12, 4,  11, 2,  15, 8,  1,  13, 1,  6,  10, 4,  13, 9,  0,  8,  6,  15, 9,  3,  8,  0,  7,  11, 4,  1,  15, 2,  14, 12, 3,  5,  11, 10, 5,  14, 2,  7,  12, };
  static byte[] s4 = { 7,  13, 13, 8,  14, 11, 3,  5,  0,  6,  6,  15, 9,  0,  10, 3,  1,  4,  2,  7,  8,  2,  5,  12, 11, 1,  12, 10, 4,  14, 15, 9,  10, 3,  6,  15, 9,  0,  0,  6,  12, 10, 11, 1,  7,  13, 13, 8,  15, 9,  1,  4,  3,  5,  14, 11, 5,  12, 2,  7,  8,  2,  4,  14, };
  static byte[] s5 = { 2,  14, 12, 11, 4,  2,  1,  12, 7,  4,  10, 7,  11, 13, 6,  1,  8,  5,  5,  0,  3,  15, 15, 10, 13, 3,  0,  9,  14, 8,  9,  6,  4,  11, 2,  8,  1,  12, 11, 7,  10, 1,  13, 14, 7,  2,  8,  13, 15, 6,  9,  15, 12, 0,  5,  9,  6,  10, 3,  4,  0,  5,  14, 3,  };
  static byte[] s6 = { 12, 10, 1,  15, 10, 4,  15, 2,  9,  7,  2,  12, 6,  9,  8,  5,  0,  6,  13, 1,  3,  13, 4,  14, 14, 0,  7,  11, 5,  3,  11, 8,  9,  4,  14, 3,  15, 2,  5,  12, 2,  9,  8,  5,  12, 15, 3,  10, 7,  11, 0,  14, 4,  1,  10, 7,  1,  6,  13, 0,  11, 8,  6,  13, };
  static byte[] s7 = { 4,  13, 11, 0,  2,  11, 14, 7,  15, 4,  0,  9,  8,  1,  13, 10, 3,  14, 12, 3,  9,  5,  7,  12, 5,  2,  10, 15, 6,  8,  1,  6,  1,  6,  4,  11, 11, 13, 13, 8,  12, 1,  3,  4,  7,  10, 14, 7,  10, 9,  15, 5,  6,  0,  8,  15, 0,  14, 5,  2,  9,  3,  2,  12, };
  static byte[] s8 = { 13, 1,  2,  15, 8,  13, 4,  8,  6,  10, 15, 3,  11, 7,  1,  4,  10, 12, 9,  5,  3,  6,  14, 11, 5,  0,  0,  14, 12, 9,  7,  2,  7,  2,  11, 1,  4,  14, 1,  7,  9,  4,  12, 10, 14, 8,  2,  13, 0,  15, 6,  12, 10, 9,  13, 0,  15, 3,  3,  5,  5,  6,  8,  11, };

  /// <summary>
  /// Does the DES PC1 permutation, taking a 64 bit key and converting it to 56 bits
  /// </summary>
  static byte[] Pc1(byte[] k) {
    return new byte[] {
      (byte)((((k[7]>>7)&1)<<7) + (((k[6]>>7)&1)<<6) + (((k[5]>>7)&1)<<5) + (((k[4]>>7)&1)<<4) + (((k[3]>>7)&1)<<3) + (((k[2]>>7)&1)<<2) + (((k[1]>>7)&1)<<1) + (((k[0]>>7)&1)<<0)),
      (byte)((((k[7]>>6)&1)<<7) + (((k[6]>>6)&1)<<6) + (((k[5]>>6)&1)<<5) + (((k[4]>>6)&1)<<4) + (((k[3]>>6)&1)<<3) + (((k[2]>>6)&1)<<2) + (((k[1]>>6)&1)<<1) + (((k[0]>>6)&1)<<0)),
      (byte)((((k[7]>>5)&1)<<7) + (((k[6]>>5)&1)<<6) + (((k[5]>>5)&1)<<5) + (((k[4]>>5)&1)<<4) + (((k[3]>>5)&1)<<3) + (((k[2]>>5)&1)<<2) + (((k[1]>>5)&1)<<1) + (((k[0]>>5)&1)<<0)),
      (byte)((((k[7]>>4)&1)<<7) + (((k[6]>>4)&1)<<6) + (((k[5]>>4)&1)<<5) + (((k[4]>>4)&1)<<4) + (((k[7]>>1)&1)<<3) + (((k[6]>>1)&1)<<2) + (((k[5]>>1)&1)<<1) + (((k[4]>>1)&1)<<0)),
      (byte)((((k[3]>>1)&1)<<7) + (((k[2]>>1)&1)<<6) + (((k[1]>>1)&1)<<5) + (((k[0]>>1)&1)<<4) + (((k[7]>>2)&1)<<3) + (((k[6]>>2)&1)<<2) + (((k[5]>>2)&1)<<1) + (((k[4]>>2)&1)<<0)),
      (byte)((((k[3]>>2)&1)<<7) + (((k[2]>>2)&1)<<6) + (((k[1]>>2)&1)<<5) + (((k[0]>>2)&1)<<4) + (((k[7]>>3)&1)<<3) + (((k[6]>>3)&1)<<2) + (((k[5]>>3)&1)<<1) + (((k[4]>>3)&1)<<0)),
      (byte)((((k[3]>>3)&1)<<7) + (((k[2]>>3)&1)<<6) + (((k[1]>>3)&1)<<5) + (((k[0]>>3)&1)<<4) + (((k[3]>>4)&1)<<3) + (((k[2]>>4)&1)<<2) + (((k[1]>>4)&1)<<1) + (((k[0]>>4)&1)<<0))
    };
  }

  /// <summary>
  /// Does the DES PC2 permutation, taking a 56bit CnDn and returning a 48bit Kn 
  /// </summary>
  static byte[] Pc2(byte[] i) {
    return new byte[] {
      (byte)((((i[1]>>2)&1)<<7) + (((i[2]>>7)&1)<<6) + (((i[1]>>5)&1)<<5) + (((i[2]>>0)&1)<<4) + (((i[0]>>7)&1)<<3) + (((i[0]>>3)&1)<<2) + (((i[0]>>5)&1)<<1) + (((i[3]>>4)&1)<<0)),
      (byte)((((i[1]>>1)&1)<<7) + (((i[0]>>2)&1)<<6) + (((i[2]>>3)&1)<<5) + (((i[1]>>6)&1)<<4) + (((i[2]>>1)&1)<<3) + (((i[2]>>5)&1)<<2) + (((i[1]>>4)&1)<<1) + (((i[0]>>4)&1)<<0)),
      (byte)((((i[3]>>6)&1)<<7) + (((i[0]>>0)&1)<<6) + (((i[1]>>0)&1)<<5) + (((i[0]>>1)&1)<<4) + (((i[3]>>5)&1)<<3) + (((i[2]>>4)&1)<<2) + (((i[1]>>3)&1)<<1) + (((i[0]>>6)&1)<<0)),
      (byte)((((i[5]>>7)&1)<<7) + (((i[6]>>4)&1)<<6) + (((i[3]>>1)&1)<<5) + (((i[4]>>3)&1)<<4) + (((i[5]>>1)&1)<<3) + (((i[6]>>1)&1)<<2) + (((i[3]>>2)&1)<<1) + (((i[4]>>0)&1)<<0)),
      (byte)((((i[6]>>5)&1)<<7) + (((i[5]>>3)&1)<<6) + (((i[4]>>7)&1)<<5) + (((i[5]>>0)&1)<<4) + (((i[5]>>4)&1)<<3) + (((i[6]>>7)&1)<<2) + (((i[4]>>1)&1)<<1) + (((i[6]>>0)&1)<<0)),
      (byte)((((i[4]>>6)&1)<<7) + (((i[6]>>3)&1)<<6) + (((i[5]>>2)&1)<<5) + (((i[5]>>6)&1)<<4) + (((i[6]>>6)&1)<<3) + (((i[4]>>4)&1)<<2) + (((i[3]>>3)&1)<<1) + (((i[3]>>0)&1)<<0))
    };
  }

  /// <summary>
  /// Does the initial Permutation on the 64 bits of the message data. oput is also 64 bits.
  /// </summary>
  static byte[] Ip(byte[] i) {
    return new byte[] {
      (byte)((((i[7]>>6)&1)<<7) + (((i[6]>>6)&1)<<6) + (((i[5]>>6)&1)<<5) + (((i[4]>>6)&1)<<4) + (((i[3]>>6)&1)<<3) + (((i[2]>>6)&1)<<2) + (((i[1]>>6)&1)<<1) + (((i[0]>>6)&1)<<0)),
      (byte)((((i[7]>>4)&1)<<7) + (((i[6]>>4)&1)<<6) + (((i[5]>>4)&1)<<5) + (((i[4]>>4)&1)<<4) + (((i[3]>>4)&1)<<3) + (((i[2]>>4)&1)<<2) + (((i[1]>>4)&1)<<1) + (((i[0]>>4)&1)<<0)),
      (byte)((((i[7]>>2)&1)<<7) + (((i[6]>>2)&1)<<6) + (((i[5]>>2)&1)<<5) + (((i[4]>>2)&1)<<4) + (((i[3]>>2)&1)<<3) + (((i[2]>>2)&1)<<2) + (((i[1]>>2)&1)<<1) + (((i[0]>>2)&1)<<0)),
      (byte)((((i[7]>>0)&1)<<7) + (((i[6]>>0)&1)<<6) + (((i[5]>>0)&1)<<5) + (((i[4]>>0)&1)<<4) + (((i[3]>>0)&1)<<3) + (((i[2]>>0)&1)<<2) + (((i[1]>>0)&1)<<1) + (((i[0]>>0)&1)<<0)),
      (byte)((((i[7]>>7)&1)<<7) + (((i[6]>>7)&1)<<6) + (((i[5]>>7)&1)<<5) + (((i[4]>>7)&1)<<4) + (((i[3]>>7)&1)<<3) + (((i[2]>>7)&1)<<2) + (((i[1]>>7)&1)<<1) + (((i[0]>>7)&1)<<0)),
      (byte)((((i[7]>>5)&1)<<7) + (((i[6]>>5)&1)<<6) + (((i[5]>>5)&1)<<5) + (((i[4]>>5)&1)<<4) + (((i[3]>>5)&1)<<3) + (((i[2]>>5)&1)<<2) + (((i[1]>>5)&1)<<1) + (((i[0]>>5)&1)<<0)),
      (byte)((((i[7]>>3)&1)<<7) + (((i[6]>>3)&1)<<6) + (((i[5]>>3)&1)<<5) + (((i[4]>>3)&1)<<4) + (((i[3]>>3)&1)<<3) + (((i[2]>>3)&1)<<2) + (((i[1]>>3)&1)<<1) + (((i[0]>>3)&1)<<0)),
      (byte)((((i[7]>>1)&1)<<7) + (((i[6]>>1)&1)<<6) + (((i[5]>>1)&1)<<5) + (((i[4]>>1)&1)<<4) + (((i[3]>>1)&1)<<3) + (((i[2]>>1)&1)<<2) + (((i[1]>>1)&1)<<1) + (((i[0]>>1)&1)<<0))
    };
  }

  /// <summary>
  /// Does the IP-1 (IP inverse) after the encryption rounds
  /// </summary>
  static byte[] IpInv(byte[] i) {
    return new byte[] {
      (byte)((((i[4]>>0)&1)<<7) + (((i[0]>>0)&1)<<6) + (((i[5]>>0)&1)<<5) + (((i[1]>>0)&1)<<4) + (((i[6]>>0)&1)<<3) + (((i[2]>>0)&1)<<2) + (((i[7]>>0)&1)<<1) + (((i[3]>>0)&1)<<0)),
      (byte)((((i[4]>>1)&1)<<7) + (((i[0]>>1)&1)<<6) + (((i[5]>>1)&1)<<5) + (((i[1]>>1)&1)<<4) + (((i[6]>>1)&1)<<3) + (((i[2]>>1)&1)<<2) + (((i[7]>>1)&1)<<1) + (((i[3]>>1)&1)<<0)),
      (byte)((((i[4]>>2)&1)<<7) + (((i[0]>>2)&1)<<6) + (((i[5]>>2)&1)<<5) + (((i[1]>>2)&1)<<4) + (((i[6]>>2)&1)<<3) + (((i[2]>>2)&1)<<2) + (((i[7]>>2)&1)<<1) + (((i[3]>>2)&1)<<0)),
      (byte)((((i[4]>>3)&1)<<7) + (((i[0]>>3)&1)<<6) + (((i[5]>>3)&1)<<5) + (((i[1]>>3)&1)<<4) + (((i[6]>>3)&1)<<3) + (((i[2]>>3)&1)<<2) + (((i[7]>>3)&1)<<1) + (((i[3]>>3)&1)<<0)),
      (byte)((((i[4]>>4)&1)<<7) + (((i[0]>>4)&1)<<6) + (((i[5]>>4)&1)<<5) + (((i[1]>>4)&1)<<4) + (((i[6]>>4)&1)<<3) + (((i[2]>>4)&1)<<2) + (((i[7]>>4)&1)<<1) + (((i[3]>>4)&1)<<0)),
      (byte)((((i[4]>>5)&1)<<7) + (((i[0]>>5)&1)<<6) + (((i[5]>>5)&1)<<5) + (((i[1]>>5)&1)<<4) + (((i[6]>>5)&1)<<3) + (((i[2]>>5)&1)<<2) + (((i[7]>>5)&1)<<1) + (((i[3]>>5)&1)<<0)),
      (byte)((((i[4]>>6)&1)<<7) + (((i[0]>>6)&1)<<6) + (((i[5]>>6)&1)<<5) + (((i[1]>>6)&1)<<4) + (((i[6]>>6)&1)<<3) + (((i[2]>>6)&1)<<2) + (((i[7]>>6)&1)<<1) + (((i[3]>>6)&1)<<0)),
      (byte)((((i[4]>>7)&1)<<7) + (((i[0]>>7)&1)<<6) + (((i[5]>>7)&1)<<5) + (((i[1]>>7)&1)<<4) + (((i[6]>>7)&1)<<3) + (((i[2]>>7)&1)<<2) + (((i[7]>>7)&1)<<1) + (((i[3]>>7)&1)<<0))
    };
  }

  /// <summary>
  /// Does the 'E' permutation
  /// Takes 32 bits i and puts 48 bits o
  /// </summary>
  static byte[] E(byte[] i) {
    return new byte[] {
      (byte)((((i[3]>>0)&1)<<7) + (((i[0]>>7)&1)<<6) + (((i[0]>>6)&1)<<5) + (((i[0]>>5)&1)<<4) + (((i[0]>>4)&1)<<3) + (((i[0]>>3)&1)<<2) + (((i[0]>>4)&1)<<1) + (((i[0]>>3)&1)<<0)),
      (byte)((((i[0]>>2)&1)<<7) + (((i[0]>>1)&1)<<6) + (((i[0]>>0)&1)<<5) + (((i[1]>>7)&1)<<4) + (((i[0]>>0)&1)<<3) + (((i[1]>>7)&1)<<2) + (((i[1]>>6)&1)<<1) + (((i[1]>>5)&1)<<0)),
      (byte)((((i[1]>>4)&1)<<7) + (((i[1]>>3)&1)<<6) + (((i[1]>>4)&1)<<5) + (((i[1]>>3)&1)<<4) + (((i[1]>>2)&1)<<3) + (((i[1]>>1)&1)<<2) + (((i[1]>>0)&1)<<1) + (((i[2]>>7)&1)<<0)),
      (byte)((((i[1]>>0)&1)<<7) + (((i[2]>>7)&1)<<6) + (((i[2]>>6)&1)<<5) + (((i[2]>>5)&1)<<4) + (((i[2]>>4)&1)<<3) + (((i[2]>>3)&1)<<2) + (((i[2]>>4)&1)<<1) + (((i[2]>>3)&1)<<0)),
      (byte)((((i[2]>>2)&1)<<7) + (((i[2]>>1)&1)<<6) + (((i[2]>>0)&1)<<5) + (((i[3]>>7)&1)<<4) + (((i[2]>>0)&1)<<3) + (((i[3]>>7)&1)<<2) + (((i[3]>>6)&1)<<1) + (((i[3]>>5)&1)<<0)),
      (byte)((((i[3]>>4)&1)<<7) + (((i[3]>>3)&1)<<6) + (((i[3]>>4)&1)<<5) + (((i[3]>>3)&1)<<4) + (((i[3]>>2)&1)<<3) + (((i[3]>>1)&1)<<2) + (((i[3]>>0)&1)<<1) + (((i[0]>>7)&1)<<0))
    };
  }

  /// <summary>
  /// Does the 'P' permutation
  /// 32 bits i, 32 bits o
  /// </summary>
  static byte[] P(byte[] i) {
    return new byte[] {
      (byte)((((i[1]>>0)&1)<<7) + (((i[0]>>1)&1)<<6) + (((i[2]>>4)&1)<<5) + (((i[2]>>3)&1)<<4) + (((i[3]>>3)&1)<<3) + (((i[1]>>4)&1)<<2) + (((i[3]>>4)&1)<<1) + (((i[2]>>7)&1)<<0)),
      (byte)((((i[0]>>7)&1)<<7) + (((i[1]>>1)&1)<<6) + (((i[2]>>1)&1)<<5) + (((i[3]>>6)&1)<<4) + (((i[0]>>3)&1)<<3) + (((i[2]>>6)&1)<<2) + (((i[3]>>1)&1)<<1) + (((i[1]>>6)&1)<<0)),
      (byte)((((i[0]>>6)&1)<<7) + (((i[0]>>0)&1)<<6) + (((i[2]>>0)&1)<<5) + (((i[1]>>2)&1)<<4) + (((i[3]>>0)&1)<<3) + (((i[3]>>5)&1)<<2) + (((i[0]>>5)&1)<<1) + (((i[1]>>7)&1)<<0)),
      (byte)((((i[2]>>5)&1)<<7) + (((i[1]>>3)&1)<<6) + (((i[3]>>2)&1)<<5) + (((i[0]>>2)&1)<<4) + (((i[2]>>2)&1)<<3) + (((i[1]>>5)&1)<<2) + (((i[0]>>4)&1)<<1) + (((i[3]>>7)&1)<<0))
    };
  }

  /// <summary>
  /// Given 56 bits, representing 28 bits of C and D, shifts both left by 1 bit
  /// </summary>
  static byte[] Left1(byte[] i) {
    return new byte[] {
      // C
      (byte)((i[0]<<1) + (i[1]>>7)),
      (byte)((i[1]<<1) + (i[2]>>7)),
      (byte)((i[2]<<1) + (i[3]>>7)),
      // 1 nibble each C / D
      (byte)(
      ((i[3]&0xf0)<<1) + ((i[0]>>7)<<4) + // the C nibble
      ((i[3]&7)<<1) + (i[4]>>7)), // the D nibble
      // D
      (byte)((i[4]<<1) + (i[5]>>7)),
      (byte)((i[5]<<1) + (i[6]>>7)),
      (byte)((i[6]<<1) + ((i[3]>>3)&1))
    };
  }

  /// <summary>
  /// Given 56 bits, representig 28 bits of C and D, shifts both left by 2 bits
  /// </summary>
  static byte[] Left2(byte[] i) {
    return new byte[] {
      // C
      (byte)((i[0]<<2) + (i[1]>>6)),
      (byte)((i[1]<<2) + (i[2]>>6)),
      (byte)((i[2]<<2) + (i[3]>>6)),
      // 1 nibble each C / D
      (byte)(
        ((i[3]&0xf0)<<2) + ((i[0]>>6)<<4) + // the C nibble
        ((i[3]&3)<<2) + (i[4]>>6)), // the D nibble
      // D
      (byte)((i[4]<<2) + (i[5]>>6)),
      (byte)((i[5]<<2) + (i[6]>>6)),
      (byte)((i[6]<<2) + ((i[3]>>2)&3))
    };
  }

  /// <summary>
  /// Expands a 64-bit key into 16 * 48 bit subkeys
  /// </summary>
  static byte[][] Expand(byte[] key) {
    // Get the 56-bit PC1 permutation
    byte[] kplus = Pc1(key);
    
    // Do the left shifts
    byte[][] keys = new byte[16][];
    keys[0] =  Left1(kplus); // Iteration 1
    keys[1] =  Left1(keys[0]);
    keys[2] =  Left2(keys[1]);
    keys[3] =  Left2(keys[2]);
    keys[4] =  Left2(keys[3]);
    keys[5] =  Left2(keys[4]);
    keys[6] =  Left2(keys[5]);
    keys[7] =  Left2(keys[6]);
    keys[8] =  Left1(keys[7]);
    keys[9] =  Left2(keys[8]);
    keys[10] = Left2(keys[9]);
    keys[11] = Left2(keys[10]);
    keys[12] = Left2(keys[11]);
    keys[13] = Left2(keys[12]);
    keys[14] = Left2(keys[13]);
    keys[15] = Left1(keys[14]);
    
    // Apply the PC2 perm to each key
    for (int i=0;i<16;i++) {
      keys[i] = Pc2(keys[i]);
    }

    return keys;
  }

  /// <summary>
  /// Split 6 bytes into 8 * 6 bit pieces
  /// </summary>
  static byte[] Split6(byte[] i) {
    // in:  11111111 11111111 11111111 11111111 11111111 11111111
    // #:     0           1       2        3         4       5
    // out: 11111122 22223333 33444444 55555566 66667777 77888888    
    return new byte[] {
      (byte)(i[0] >> 2),
      (byte)(((i[0]&3)<<4) + (i[1]>>4)),
      (byte)(((i[1]&15)<<2) + (i[2]>>6)),
      (byte)(i[2]&63),
      (byte)(i[3]>>2),
      (byte)(((i[3]&3)<<4) + (i[4]>>4)),
      (byte)(((i[4]&15)<<2) + (i[5]>>6)),
      (byte)(i[5]&63)
    };
  }

  /// <summary>
  /// Takes 8 * 6-bit values, does a s-box lookup which returns 4 bits each,
  /// and joins the 8*4 bits to return 4 bytes
  /// </summary>
  static byte[] Sbox(byte[] i)  {
    return new byte[] {
      (byte)((s1[i[0]]<<4) + s2[i[1]]),
      (byte)((s3[i[2]]<<4) + s4[i[3]]),
      (byte)((s5[i[4]]<<4) + s6[i[5]]),
      (byte)((s7[i[6]]<<4) + s8[i[7]])
    };
  }

  /// <summary>
  /// Takes 32 bits input, 48 bits key Kn, gives 32 bits output
  /// Does: P(S(Kn ^ E(R))), where R = in = R(n-1)
  /// </summary>
  static byte[] F(byte[] r, byte[] subkey) {
    byte[] er = E(r);        // Expand using E to 48 bits
    byte[] x  = Xor(er,subkey); // Now XOR the output of E with the key Kn
    byte[] b  = Split6(x);   // Split it into 8 blocks of 6-bits
    byte[] s  = Sbox(b);     // Now do the 'S box' lookup and return it to 32 bits
    return P(s);             // Now do final P permutation
  }

  /// <summary>
  /// Xors all elements of two arrays, returning the resultant array
  /// </summary>
  static byte[] Xor(byte[] a, byte[] b)
  {
    byte[] o = new byte[a.Length];
    for (int i = 0; i < a.Length; i++)
      o[i] = (byte)(a[i] ^ b[i]);
    return o;
  }

  /// <summary>
  /// Xor's the 4 bytes in a and b, outputting to o
  /// </summary>
  static void Xor(byte[] a, byte[] b, byte[] o)
  {
    o[0] = (byte)(a[0] ^ b[0]);
    o[1] = (byte)(a[1] ^ b[1]);
    o[2] = (byte)(a[2] ^ b[2]);
    o[3] = (byte)(a[3] ^ b[3]);
  }

  /// <summary>
  /// Execute a DES round of encryption, eg:
  /// L1 = R0
  /// R1 = L0 + f(R0,K1)
  /// </summary>
  static void Round(byte[] l, byte[] r, byte[] subkey) {
    byte[] l_old = new byte[4]; // Keep a copy of the old values
    byte[] r_old = new byte[4];
    Array.Copy(l,l_old,4);
    Array.Copy(r,r_old,4);
    Array.Copy(r_old,l,4);      // L = R
    byte[] f = F(r_old,subkey); // f(R0,K1)
    Xor(l_old,f,r);             // R = L ^ f
  }

  /// <summary>
  /// Split an array into two halves
  /// </summary>
  static void Split(byte[] i, out byte[] l, out byte[] r) {
    l = new byte[i.Length / 2];
    r = new byte[i.Length / 2];
    Array.Copy(i, 0, l, 0, i.Length / 2);
    Array.Copy(i, i.Length/2, r, 0, i.Length / 2);
  }

  /// <summary>
  /// Join two arrays in right then left order
  /// </summary>
  static byte[] ReverseSides(byte[] l, byte[] r) {
    byte[] o = new byte[l.Length + r.Length];
    Array.Copy(r, o, r.Length);
    Array.Copy(l, 0, o, r.Length, l.Length);
    return o;
  }

  /// <summary>
  /// Takes a 64-bit message and subkeys, DES encrypts it
  /// Outputs 64 bits to out
  /// </summary>
  static byte[] DesEncrypt(byte[] m, byte[][] subkeys) {
    byte[] i = Ip(m);               // Perform the IP transform
    byte[] l, r;
    Split(i, out l, out r);         // Split into L & R halves
    for (int rnd=0;rnd<=15;rnd++)   // Iterate the rounds
      Round(l,r,subkeys[rnd]);      // Perform l=r, r=l^f(r,subkey)
    byte[] rl = ReverseSides(l,r);  // Reverse, the sides
    return IpInv(rl);               // Perform the IP-1 transform
  }

  /// <summary>
  /// Takes a 64-bit message and subkeys, DES decrypts it
  /// Outputs 64 bits to out
  /// </summary>
  static byte[] DesDecrypt(byte[] m, byte[][] subkeys)
  {
    byte[] i = Ip(m);               // Perform the IP transform
    byte[] l, r;
    Split(i, out l, out r);         // Split into L & R halves
    for (int rnd=15;rnd>=0;rnd--)   // Iterate the rounds
      Round(l, r, subkeys[rnd]);    // Perform l=r, r=l^f(r,subkey)
    byte[] rl = ReverseSides(l, r); // Reverse, the sides
    return IpInv(rl);               // Perform the IP-1 transform
  }

  /// <summary>
  /// Takes a 64 bit message and a 128 bit key, and triple des encrypts it
  /// </summary>
  static byte[] TripleDesEncrypt(byte[] m,byte[] key) {
    byte[] a, b;
    Split(key,out a,out b);   // Split the 128 bit key into two DES keys
    byte[][] sa = Expand(a);  // Expand from the key to the subkeys
    byte[][] sb = Expand(b);  
    byte[] o;
    o = DesEncrypt(m,sa);     // Encrypt with the A key
    o = DesDecrypt(o,sb);     // Decrypt with B
    o = DesEncrypt(o,sa);     // Encrypt with A
    return o;
  }

  /// <summary>
  /// Takes a 64 bit message and a 128 bit key, and triple des decrypts it
  /// </summary>
  static byte[] TripleDesDecrypt(byte[] m,byte[] key) {
    byte[] a, b;
    Split(key,out a,out b);   // Split the 128 bit key into two DES keys
    byte[][] sa = Expand(a);  // Expand from the key to the subkeys
    byte[][] sb = Expand(b);  
    byte[] o;
    o = DesDecrypt(m,sa);     // Decrypt with the A key
    o = DesEncrypt(o,sb);     // Encrypt with B
    o = DesDecrypt(o,sa);     // Decrypt with A
    return o;
  }
 
  /// <summary>
  /// Convert a hex string to bytes
  /// </summary>
  static byte[] ToBytes(string str) {
    byte[] b = new byte[str.Length / 2];
    for (int i = 0; i < str.Length / 2; i++)
      b[i] = (byte)Convert.ToInt32(str.Substring(i*2,2), 16);
    return b;
  }

  /// <summary>
  /// Pretty-print a byte array
  /// </summary>
  static void Pretty(string label, byte[] a) {
    string arr="";
    foreach (byte b in a) {
      if (arr.Length > 0) arr += "-";
      arr += b.ToString("X2");
    }
    Console.WriteLine("{0}:\r\n {1}", label, arr);
  }

  /// <summary>
  /// Test the crypto
  /// </summary>
  static void Main(string[] args) {
    Console.WriteLine("Test DES");

    byte[] key = ToBytes("133457799BBCDFF1");
    byte[] msg = ToBytes("0123456789ABCDEF");
    
    byte[][] subkeys = Expand(key);
    byte[] crypt = DesEncrypt(msg,subkeys);
    byte[] clear = DesDecrypt(crypt,subkeys);
    
    Pretty("Key", key);
    Pretty("Original message", msg);
    Pretty("Encrypted (should be 85-E8...B4-05)", crypt);
    Pretty("Decrypted (should match the original)", clear);
    
    
    Console.WriteLine("\r\nTest Triple-DES");
    byte[] k3d = ToBytes("11223344556677898798794535213544");
    byte[] m3d = ToBytes("1234567890ABCDEF");
    byte[] e3d = TripleDesEncrypt(m3d,k3d);
    byte[] d3d = TripleDesDecrypt(e3d, k3d);
    Pretty("Encrypted (should be 3A-3A...BB-DC)", e3d);
    Pretty("Decrypted (should be 12-34...CD-EF)", d3d);
  }
}