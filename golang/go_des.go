// Simple, thoroughly commented implementation of DES / Triple DES using Google Go aka Golang
// Chris Hulbert - chris.hulbert@gmail.com - http://splinter.com.au/blog - http://github.com/chrishulbert/crypto
// Reference: http://orlingrabbe.com/des.htm

package main
import "fmt"

// S-box lookups transformed so you don't have to figure out rows and columns
var s1 = [...]byte{ 14, 0,  4,  15, 13, 7,  1,  4,  2,  14, 15, 2,  11, 13, 8,  1,  3,  10, 10, 6,  6,  12, 12, 11, 5,  9,  9,  5,  0,  3,  7,  8,  4,  15, 1,  12, 14, 8,  8,  2,  13, 4,  6,  9,  2,  1,  11, 7,  15, 5,  12, 11, 9,  3,  7,  14, 3,  10, 10, 0,  5,  6,  0,  13, };
var s2 = [...]byte{ 15, 3,  1,  13, 8,  4,  14, 7,  6,  15, 11, 2,  3,  8,  4,  14, 9,  12, 7,  0,  2,  1,  13, 10, 12, 6,  0,  9,  5,  11, 10, 5,  0,  13, 14, 8,  7,  10, 11, 1,  10, 3,  4,  15, 13, 4,  1,  2,  5,  11, 8,  6,  12, 7,  6,  12, 9,  0,  3,  5,  2,  14, 15, 9,  };
var s3 = [...]byte{ 10, 13, 0,  7,  9,  0,  14, 9,  6,  3,  3,  4,  15, 6,  5,  10, 1,  2,  13, 8,  12, 5,  7,  14, 11, 12, 4,  11, 2,  15, 8,  1,  13, 1,  6,  10, 4,  13, 9,  0,  8,  6,  15, 9,  3,  8,  0,  7,  11, 4,  1,  15, 2,  14, 12, 3,  5,  11, 10, 5,  14, 2,  7,  12, };
var s4 = [...]byte{ 7,  13, 13, 8,  14, 11, 3,  5,  0,  6,  6,  15, 9,  0,  10, 3,  1,  4,  2,  7,  8,  2,  5,  12, 11, 1,  12, 10, 4,  14, 15, 9,  10, 3,  6,  15, 9,  0,  0,  6,  12, 10, 11, 1,  7,  13, 13, 8,  15, 9,  1,  4,  3,  5,  14, 11, 5,  12, 2,  7,  8,  2,  4,  14, };
var s5 = [...]byte{ 2,  14, 12, 11, 4,  2,  1,  12, 7,  4,  10, 7,  11, 13, 6,  1,  8,  5,  5,  0,  3,  15, 15, 10, 13, 3,  0,  9,  14, 8,  9,  6,  4,  11, 2,  8,  1,  12, 11, 7,  10, 1,  13, 14, 7,  2,  8,  13, 15, 6,  9,  15, 12, 0,  5,  9,  6,  10, 3,  4,  0,  5,  14, 3,  };
var s6 = [...]byte{ 12, 10, 1,  15, 10, 4,  15, 2,  9,  7,  2,  12, 6,  9,  8,  5,  0,  6,  13, 1,  3,  13, 4,  14, 14, 0,  7,  11, 5,  3,  11, 8,  9,  4,  14, 3,  15, 2,  5,  12, 2,  9,  8,  5,  12, 15, 3,  10, 7,  11, 0,  14, 4,  1,  10, 7,  1,  6,  13, 0,  11, 8,  6,  13, };
var s7 = [...]byte{ 4,  13, 11, 0,  2,  11, 14, 7,  15, 4,  0,  9,  8,  1,  13, 10, 3,  14, 12, 3,  9,  5,  7,  12, 5,  2,  10, 15, 6,  8,  1,  6,  1,  6,  4,  11, 11, 13, 13, 8,  12, 1,  3,  4,  7,  10, 14, 7,  10, 9,  15, 5,  6,  0,  8,  15, 0,  14, 5,  2,  9,  3,  2,  12, };
var s8 = [...]byte{ 13, 1,  2,  15, 8,  13, 4,  8,  6,  10, 15, 3,  11, 7,  1,  4,  10, 12, 9,  5,  3,  6,  14, 11, 5,  0,  0,  14, 12, 9,  7,  2,  7,  2,  11, 1,  4,  14, 1,  7,  9,  4,  12, 10, 14, 8,  2,  13, 0,  15, 6,  12, 10, 9,  13, 0,  15, 3,  3,  5,  5,  6,  8,  11, };

// Does the DES PC1 permutation, taking a 64 bit key and converting it to 56 bits
func pc1(k []byte) (out []byte) {
  out = make([]byte, 7)
  out[0] = (((k[7]>>7)&1)<<7) + (((k[6]>>7)&1)<<6) + (((k[5]>>7)&1)<<5) + (((k[4]>>7)&1)<<4) + (((k[3]>>7)&1)<<3) + (((k[2]>>7)&1)<<2) + (((k[1]>>7)&1)<<1) + (((k[0]>>7)&1)<<0)
  out[1] = (((k[7]>>6)&1)<<7) + (((k[6]>>6)&1)<<6) + (((k[5]>>6)&1)<<5) + (((k[4]>>6)&1)<<4) + (((k[3]>>6)&1)<<3) + (((k[2]>>6)&1)<<2) + (((k[1]>>6)&1)<<1) + (((k[0]>>6)&1)<<0)
  out[2] = (((k[7]>>5)&1)<<7) + (((k[6]>>5)&1)<<6) + (((k[5]>>5)&1)<<5) + (((k[4]>>5)&1)<<4) + (((k[3]>>5)&1)<<3) + (((k[2]>>5)&1)<<2) + (((k[1]>>5)&1)<<1) + (((k[0]>>5)&1)<<0)
  out[3] = (((k[7]>>4)&1)<<7) + (((k[6]>>4)&1)<<6) + (((k[5]>>4)&1)<<5) + (((k[4]>>4)&1)<<4) + (((k[7]>>1)&1)<<3) + (((k[6]>>1)&1)<<2) + (((k[5]>>1)&1)<<1) + (((k[4]>>1)&1)<<0)
  out[4] = (((k[3]>>1)&1)<<7) + (((k[2]>>1)&1)<<6) + (((k[1]>>1)&1)<<5) + (((k[0]>>1)&1)<<4) + (((k[7]>>2)&1)<<3) + (((k[6]>>2)&1)<<2) + (((k[5]>>2)&1)<<1) + (((k[4]>>2)&1)<<0)
  out[5] = (((k[3]>>2)&1)<<7) + (((k[2]>>2)&1)<<6) + (((k[1]>>2)&1)<<5) + (((k[0]>>2)&1)<<4) + (((k[7]>>3)&1)<<3) + (((k[6]>>3)&1)<<2) + (((k[5]>>3)&1)<<1) + (((k[4]>>3)&1)<<0)
  out[6] = (((k[3]>>3)&1)<<7) + (((k[2]>>3)&1)<<6) + (((k[1]>>3)&1)<<5) + (((k[0]>>3)&1)<<4) + (((k[3]>>4)&1)<<3) + (((k[2]>>4)&1)<<2) + (((k[1]>>4)&1)<<1) + (((k[0]>>4)&1)<<0)
  return
}

// Does the DES PC2 permutation, taking a 56bit CnDn and returning a 48bit Kn 
func pc2(in []byte) (out []byte) {
  out = make([]byte, 6)
  out[0] = (((in[1]>>2)&1)<<7) + (((in[2]>>7)&1)<<6) + (((in[1]>>5)&1)<<5) + (((in[2]>>0)&1)<<4) + (((in[0]>>7)&1)<<3) + (((in[0]>>3)&1)<<2) + (((in[0]>>5)&1)<<1) + (((in[3]>>4)&1)<<0);
  out[1] = (((in[1]>>1)&1)<<7) + (((in[0]>>2)&1)<<6) + (((in[2]>>3)&1)<<5) + (((in[1]>>6)&1)<<4) + (((in[2]>>1)&1)<<3) + (((in[2]>>5)&1)<<2) + (((in[1]>>4)&1)<<1) + (((in[0]>>4)&1)<<0);
  out[2] = (((in[3]>>6)&1)<<7) + (((in[0]>>0)&1)<<6) + (((in[1]>>0)&1)<<5) + (((in[0]>>1)&1)<<4) + (((in[3]>>5)&1)<<3) + (((in[2]>>4)&1)<<2) + (((in[1]>>3)&1)<<1) + (((in[0]>>6)&1)<<0);
  out[3] = (((in[5]>>7)&1)<<7) + (((in[6]>>4)&1)<<6) + (((in[3]>>1)&1)<<5) + (((in[4]>>3)&1)<<4) + (((in[5]>>1)&1)<<3) + (((in[6]>>1)&1)<<2) + (((in[3]>>2)&1)<<1) + (((in[4]>>0)&1)<<0);
  out[4] = (((in[6]>>5)&1)<<7) + (((in[5]>>3)&1)<<6) + (((in[4]>>7)&1)<<5) + (((in[5]>>0)&1)<<4) + (((in[5]>>4)&1)<<3) + (((in[6]>>7)&1)<<2) + (((in[4]>>1)&1)<<1) + (((in[6]>>0)&1)<<0);
  out[5] = (((in[4]>>6)&1)<<7) + (((in[6]>>3)&1)<<6) + (((in[5]>>2)&1)<<5) + (((in[5]>>6)&1)<<4) + (((in[6]>>6)&1)<<3) + (((in[4]>>4)&1)<<2) + (((in[3]>>3)&1)<<1) + (((in[3]>>0)&1)<<0);
  return
}

// Does the Initial Permutation on the 64 bits of the message data. Output is also 64 bits.
func ip(in []byte) (out []byte) {
  out = make([]byte,8)
  out[0] = (((in[7]>>6)&1)<<7) + (((in[6]>>6)&1)<<6) + (((in[5]>>6)&1)<<5) + (((in[4]>>6)&1)<<4) + (((in[3]>>6)&1)<<3) + (((in[2]>>6)&1)<<2) + (((in[1]>>6)&1)<<1) + (((in[0]>>6)&1)<<0);
  out[1] = (((in[7]>>4)&1)<<7) + (((in[6]>>4)&1)<<6) + (((in[5]>>4)&1)<<5) + (((in[4]>>4)&1)<<4) + (((in[3]>>4)&1)<<3) + (((in[2]>>4)&1)<<2) + (((in[1]>>4)&1)<<1) + (((in[0]>>4)&1)<<0);
  out[2] = (((in[7]>>2)&1)<<7) + (((in[6]>>2)&1)<<6) + (((in[5]>>2)&1)<<5) + (((in[4]>>2)&1)<<4) + (((in[3]>>2)&1)<<3) + (((in[2]>>2)&1)<<2) + (((in[1]>>2)&1)<<1) + (((in[0]>>2)&1)<<0);
  out[3] = (((in[7]>>0)&1)<<7) + (((in[6]>>0)&1)<<6) + (((in[5]>>0)&1)<<5) + (((in[4]>>0)&1)<<4) + (((in[3]>>0)&1)<<3) + (((in[2]>>0)&1)<<2) + (((in[1]>>0)&1)<<1) + (((in[0]>>0)&1)<<0);
  out[4] = (((in[7]>>7)&1)<<7) + (((in[6]>>7)&1)<<6) + (((in[5]>>7)&1)<<5) + (((in[4]>>7)&1)<<4) + (((in[3]>>7)&1)<<3) + (((in[2]>>7)&1)<<2) + (((in[1]>>7)&1)<<1) + (((in[0]>>7)&1)<<0);
  out[5] = (((in[7]>>5)&1)<<7) + (((in[6]>>5)&1)<<6) + (((in[5]>>5)&1)<<5) + (((in[4]>>5)&1)<<4) + (((in[3]>>5)&1)<<3) + (((in[2]>>5)&1)<<2) + (((in[1]>>5)&1)<<1) + (((in[0]>>5)&1)<<0);
  out[6] = (((in[7]>>3)&1)<<7) + (((in[6]>>3)&1)<<6) + (((in[5]>>3)&1)<<5) + (((in[4]>>3)&1)<<4) + (((in[3]>>3)&1)<<3) + (((in[2]>>3)&1)<<2) + (((in[1]>>3)&1)<<1) + (((in[0]>>3)&1)<<0);
  out[7] = (((in[7]>>1)&1)<<7) + (((in[6]>>1)&1)<<6) + (((in[5]>>1)&1)<<5) + (((in[4]>>1)&1)<<4) + (((in[3]>>1)&1)<<3) + (((in[2]>>1)&1)<<2) + (((in[1]>>1)&1)<<1) + (((in[0]>>1)&1)<<0);
  return
}

// Does the IP-1 after the encryption rounds
func ip_reverse(in []byte) (out []byte) {
  out = make([]byte,8)
  out[0] = (((in[4]>>0)&1)<<7) + (((in[0]>>0)&1)<<6) + (((in[5]>>0)&1)<<5) + (((in[1]>>0)&1)<<4) + (((in[6]>>0)&1)<<3) + (((in[2]>>0)&1)<<2) + (((in[7]>>0)&1)<<1) + (((in[3]>>0)&1)<<0);
  out[1] = (((in[4]>>1)&1)<<7) + (((in[0]>>1)&1)<<6) + (((in[5]>>1)&1)<<5) + (((in[1]>>1)&1)<<4) + (((in[6]>>1)&1)<<3) + (((in[2]>>1)&1)<<2) + (((in[7]>>1)&1)<<1) + (((in[3]>>1)&1)<<0);
  out[2] = (((in[4]>>2)&1)<<7) + (((in[0]>>2)&1)<<6) + (((in[5]>>2)&1)<<5) + (((in[1]>>2)&1)<<4) + (((in[6]>>2)&1)<<3) + (((in[2]>>2)&1)<<2) + (((in[7]>>2)&1)<<1) + (((in[3]>>2)&1)<<0);
  out[3] = (((in[4]>>3)&1)<<7) + (((in[0]>>3)&1)<<6) + (((in[5]>>3)&1)<<5) + (((in[1]>>3)&1)<<4) + (((in[6]>>3)&1)<<3) + (((in[2]>>3)&1)<<2) + (((in[7]>>3)&1)<<1) + (((in[3]>>3)&1)<<0);
  out[4] = (((in[4]>>4)&1)<<7) + (((in[0]>>4)&1)<<6) + (((in[5]>>4)&1)<<5) + (((in[1]>>4)&1)<<4) + (((in[6]>>4)&1)<<3) + (((in[2]>>4)&1)<<2) + (((in[7]>>4)&1)<<1) + (((in[3]>>4)&1)<<0);
  out[5] = (((in[4]>>5)&1)<<7) + (((in[0]>>5)&1)<<6) + (((in[5]>>5)&1)<<5) + (((in[1]>>5)&1)<<4) + (((in[6]>>5)&1)<<3) + (((in[2]>>5)&1)<<2) + (((in[7]>>5)&1)<<1) + (((in[3]>>5)&1)<<0);
  out[6] = (((in[4]>>6)&1)<<7) + (((in[0]>>6)&1)<<6) + (((in[5]>>6)&1)<<5) + (((in[1]>>6)&1)<<4) + (((in[6]>>6)&1)<<3) + (((in[2]>>6)&1)<<2) + (((in[7]>>6)&1)<<1) + (((in[3]>>6)&1)<<0);
  out[7] = (((in[4]>>7)&1)<<7) + (((in[0]>>7)&1)<<6) + (((in[5]>>7)&1)<<5) + (((in[1]>>7)&1)<<4) + (((in[6]>>7)&1)<<3) + (((in[2]>>7)&1)<<2) + (((in[7]>>7)&1)<<1) + (((in[3]>>7)&1)<<0);
  return
}

// Does the 'E' permutation
// Takes 32 bits in and puts 48 bits out
func e(in []byte) (out []byte) {
  out = make ([]byte,6)
  out[0] = (((in[3]>>0)&1)<<7) + (((in[0]>>7)&1)<<6) + (((in[0]>>6)&1)<<5) + (((in[0]>>5)&1)<<4) + (((in[0]>>4)&1)<<3) + (((in[0]>>3)&1)<<2) + (((in[0]>>4)&1)<<1) + (((in[0]>>3)&1)<<0);
  out[1] = (((in[0]>>2)&1)<<7) + (((in[0]>>1)&1)<<6) + (((in[0]>>0)&1)<<5) + (((in[1]>>7)&1)<<4) + (((in[0]>>0)&1)<<3) + (((in[1]>>7)&1)<<2) + (((in[1]>>6)&1)<<1) + (((in[1]>>5)&1)<<0);
  out[2] = (((in[1]>>4)&1)<<7) + (((in[1]>>3)&1)<<6) + (((in[1]>>4)&1)<<5) + (((in[1]>>3)&1)<<4) + (((in[1]>>2)&1)<<3) + (((in[1]>>1)&1)<<2) + (((in[1]>>0)&1)<<1) + (((in[2]>>7)&1)<<0);
  out[3] = (((in[1]>>0)&1)<<7) + (((in[2]>>7)&1)<<6) + (((in[2]>>6)&1)<<5) + (((in[2]>>5)&1)<<4) + (((in[2]>>4)&1)<<3) + (((in[2]>>3)&1)<<2) + (((in[2]>>4)&1)<<1) + (((in[2]>>3)&1)<<0);
  out[4] = (((in[2]>>2)&1)<<7) + (((in[2]>>1)&1)<<6) + (((in[2]>>0)&1)<<5) + (((in[3]>>7)&1)<<4) + (((in[2]>>0)&1)<<3) + (((in[3]>>7)&1)<<2) + (((in[3]>>6)&1)<<1) + (((in[3]>>5)&1)<<0);
  out[5] = (((in[3]>>4)&1)<<7) + (((in[3]>>3)&1)<<6) + (((in[3]>>4)&1)<<5) + (((in[3]>>3)&1)<<4) + (((in[3]>>2)&1)<<3) + (((in[3]>>1)&1)<<2) + (((in[3]>>0)&1)<<1) + (((in[0]>>7)&1)<<0);
  return
}

// Does the 'P' permutation
// 32 bits in, 32 bits out
func p(in []byte) (out []byte) {
  out = make ([]byte,4)
  out[0] = (((in[1]>>0)&1)<<7) + (((in[0]>>1)&1)<<6) + (((in[2]>>4)&1)<<5) + (((in[2]>>3)&1)<<4) + (((in[3]>>3)&1)<<3) + (((in[1]>>4)&1)<<2) + (((in[3]>>4)&1)<<1) + (((in[2]>>7)&1)<<0);
  out[1] = (((in[0]>>7)&1)<<7) + (((in[1]>>1)&1)<<6) + (((in[2]>>1)&1)<<5) + (((in[3]>>6)&1)<<4) + (((in[0]>>3)&1)<<3) + (((in[2]>>6)&1)<<2) + (((in[3]>>1)&1)<<1) + (((in[1]>>6)&1)<<0);
  out[2] = (((in[0]>>6)&1)<<7) + (((in[0]>>0)&1)<<6) + (((in[2]>>0)&1)<<5) + (((in[1]>>2)&1)<<4) + (((in[3]>>0)&1)<<3) + (((in[3]>>5)&1)<<2) + (((in[0]>>5)&1)<<1) + (((in[1]>>7)&1)<<0);
  out[3] = (((in[2]>>5)&1)<<7) + (((in[1]>>3)&1)<<6) + (((in[3]>>2)&1)<<5) + (((in[0]>>2)&1)<<4) + (((in[2]>>2)&1)<<3) + (((in[1]>>5)&1)<<2) + (((in[0]>>4)&1)<<1) + (((in[3]>>7)&1)<<0);
  return
}

// Split 6 bytes into 8 * 6 bit pieces
func split6(in []byte) (out []byte) {
  // in:  11111111 11111111 11111111 11111111 11111111 11111111
  // #:     0           1       2        3         4       5
  // out: 11111122 22223333 33444444 55555566 66667777 77888888    
  out=make([]byte,8)
  out[0] = in[0]>>2;
  out[1] = ((in[0]&3)<<4) + (in[1]>>4);
  out[2] = ((in[1]&15)<<2) + (in[2]>>6);
  out[3] = in[2]&63;
  out[4] = in[3]>>2;
  out[5] = ((in[3]&3)<<4) + (in[4]>>4);
  out[6] = ((in[4]&15)<<2) + (in[5]>>6);
  out[7] = in[5]&63;
  return
}

// Xor's 2 arrays
func xor(a []byte,b []byte) (out []byte) {
  out = make([]byte,len(a))
  for i:=0;i<len(a);i++ {
    out[i] = a[i] ^ b[i]
  }
  return
}

// Takes 8 * 6-bit values, does a s-box lookup which returns 4 bits each,
// and joins the 8*4 bits to return 4 bytes
func sbox(in []byte) (s []byte) {
  s = make([]byte,4)
  s[0] = (s1[in[0]]<<4) + s2[in[1]]
  s[1] = (s3[in[2]]<<4) + s4[in[3]]
  s[2] = (s5[in[4]]<<4) + s6[in[5]]
  s[3] = (s7[in[6]]<<4) + s8[in[7]]
  return
}

// Takes 32 bits input, 48 bits key Kn, gives 32 bits output
// Does: P(S(Kn ^ E(R))), where R = in = R(n-1)
func f(in []byte,key []byte) (out []byte) {
  er := e(in)       // Expand using E to 48 bits
  x  := xor(er,key) // Now XOR the output of E with the key Kn
  b  := split6(x)   // Split it into 8 blocks of 6-bits
  s  := sbox(b)     // Now do the 'S box' lookup and return it to 32 bits
  return p(s)       // Now do final P permutation
}

// Given 56 bits, representing 28 bits of C and D, shifts both left by 1 bit
func left1(in []byte) (out []byte) {
  out = make([]byte, 7)
  // C
  out[0]=(in[0]<<1) + (in[1]>>7)
  out[1]=(in[1]<<1) + (in[2]>>7)
  out[2]=(in[2]<<1) + (in[3]>>7)
  // 1 nibble each C / D
  out[3]=
    ((in[3]&0xf0)<<1) + ((in[0]>>7)<<4) + // the C nibble
    ((in[3]&7)<<1) + (in[4]>>7) // the D nibble
  // D
  out[4]=(in[4]<<1) + (in[5]>>7)
  out[5]=(in[5]<<1) + (in[6]>>7)
  out[6]=(in[6]<<1) + ((in[3]>>3)&1)
  return
}

// Given 56 bits, representing 28 bits of C and D, shifts both left by 2 bits
func left2(in []byte) (out []byte) {
  out = make([]byte, 7)
  // C
  out[0]=(in[0]<<2) + (in[1]>>6)
  out[1]=(in[1]<<2) + (in[2]>>6)
  out[2]=(in[2]<<2) + (in[3]>>6)
  // 1 nibble each C / D
  out[3]=
    ((in[3]&0xf0)<<2) + ((in[0]>>6)<<4) + // the C nibble
    ((in[3]&3)<<2) + (in[4]>>6) // the D nibble
  // D
  out[4]=(in[4]<<2) + (in[5]>>6)
  out[5]=(in[5]<<2) + (in[6]>>6)
  out[6]=(in[6]<<2) + ((in[3]>>2)&3)
  return
}

// Expands a 64-bit key into 16 * 48 bit subkeys
func expand(key []byte) (keys [][]byte) {
  // Get the 56-bit PC1 permutation
  kplus := pc1(key)
  
  // Do the left shifts
  keys = make([][]byte,16)
  keys[0] =  left1(kplus) // Iteration 1
  keys[1] =  left1(keys[0])
  keys[2] =  left2(keys[1])
  keys[3] =  left2(keys[2])
  keys[4] =  left2(keys[3])
  keys[5] =  left2(keys[4])
  keys[6] =  left2(keys[5])
  keys[7] =  left2(keys[6])
  keys[8] =  left1(keys[7])
  keys[9] =  left2(keys[8])
  keys[10] = left2(keys[9])
  keys[11] = left2(keys[10])
  keys[12] = left2(keys[11])
  keys[13] = left2(keys[12])
  keys[14] = left2(keys[13])
  keys[15] = left1(keys[14])
  
  // Apply the PC2 perm to each key
  for i:=0;i<16;i++ {
    keys[i] = pc2(keys[i])
  }
  
  return
}

// Splits an array in two halves
func split(in []byte) (a []byte, b []byte) {
  l := len(in)/2
  a = make([]byte,l)
  b = make([]byte,l)
  copy (a,in[0:l])
  copy (b,in[l:])
  return
}

// Execute a DES round of encryption, eg:
// L1 = R0
// R1 = L0 + f(R0,K1)
func round(l_in []byte, r_in[] byte, subkey[] byte) (l_out[]byte, r_out[]byte) {
  l_out = r_in
  r_out = xor(l_in, f(r_in, subkey))
  return
}

// Joins two arrays
func join(a []byte, b []byte) (out []byte) {
  out = make([]byte,len(a)+len(b))
  copy(out,a)
  copy(out[len(a):],b)
  return
}

// Takes a 64-bit message and subkeys
// Outputs 64 bits to out
func des_encrypt(m []byte,subkeys [][]byte) (out []byte) {
  i := ip(m)      // Perform the IP transform
  l,r := split(i) // Split the result into left and right sides
  for rnd:=0;rnd<=15;rnd++ {      // Iterate the rounds
    l,r = round(l,r,subkeys[rnd]) // Perform l=r, r=l^f(r,subkey)
  }
  rl := join(r,l)       // Rejoin, but reverse, the sides
  return ip_reverse(rl) // Perform the IP-1 transform
}

// Takes a 64-bit message and subkeys
// Outputs 64 bits to out
// This is exactly the same as des_encrypt but the subkeys are reversed
func des_decrypt(m []byte,subkeys [][]byte) (out []byte) {
  i := ip(m)      // Perform the IP transform
  l,r := split(i) // Split the result into left and right sides
  for rnd:=15;rnd>=0;rnd-- {      // Iterate the rounds in reverse for decrypting
    l,r = round(l,r,subkeys[rnd]) // Perform l=r, r=l^f(r,subkey)
  }
  rl := join(r,l)       // Rejoin, but reverse, the sides
  return ip_reverse(rl) // Perform the IP-1 transform
}

// Takes a 64 bit message and a 128 bit key, and triple des encrypts it
func tripledes_encrypt(m []byte,key []byte) (out []byte) {
  a,b := split(key)         // Split the 128 bit key into two DES keys
  sa := expand(a)           // Expand from the key to the subkeys
  sb := expand(b)           // Expand from the key to the subkeys
  out = des_encrypt(m,sa)   // Encrypt with the A key
  out = des_decrypt(out,sb) // Decrypt with B
  out = des_encrypt(out,sa) // Encrypt with A
  return
}

// Takes a 64 bit message and a 128 bit key, and triple des decrypts it
func tripledes_decrypt(m []byte,key []byte) (out []byte) {
  a,b := split(key)         // Split the 128 bit key into two DES keys
  sa := expand(a)           // Expand from the key to the subkeys
  sb := expand(b)           // Expand from the key to the subkeys
  out = des_decrypt(m,sa)   // Decrypt with the A key
  out = des_encrypt(out,sb) // Encrypt with B
  out = des_decrypt(out,sa) // Decrypt with A
  return
}

// Convert a string eg 85E5A3D7356A61E29A8AFA559AD67102 into an array of bytes
func to_bytes(s string) []byte {
  l := len(s)/2
  b := make([]byte,l)
  for i:=0;i<l;i++ {
    fmt.Sscanf(s[i*2:i*2+2],"%x", &b[i])
  }
  return b
}

// Pretty-print an array
func pretty(label string, arr []byte) {
  var s string=""
  for i,b := range arr {
    s += fmt.Sprintf("%02X",b)
    if i<len(arr)-1 {
      s += "-"
    }
  }
  fmt.Printf("%s:\r\n%s\r\n", label, s)
}

// Test the crypto implementation
func main() {
  println("Test DES");

  key := to_bytes("133457799BBCDFF1")
  msg := to_bytes("0123456789ABCDEF")
  
  subkeys := expand(key)
  crypt := des_encrypt(msg,subkeys)
  clear := des_decrypt(crypt,subkeys)
  
  pretty("Key", key)
  pretty("Message", msg)
  pretty("Encrypted (should be 85-E8-13-54-0F-0A-B4-05)", crypt)
  pretty("Decrypted", clear)
  
  
  println("\r\nTest Triple-DES")
  k3d := to_bytes("11223344556677898798794535213544")
  m3d := to_bytes("1234567890ABCDEF")
  e3d := tripledes_encrypt(m3d,k3d)
  d3d := tripledes_decrypt(e3d,k3d)
  pretty("Encrypted (should be 3A-3A-CE-65-0D-B3-BB-DC)",e3d);
  pretty("Decrypted (should be 12-34-56-78-90-AB-CD-EF)",d3d);
}
