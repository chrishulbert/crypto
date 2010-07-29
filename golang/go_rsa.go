// Simple, thoroughly commented implementation of 1024-bit RSA using Google Go aka Golang
// Chris Hulbert - chris.hulbert@gmail.com - http://splinter.com.au/blog
// http://github.com/chrishulbert/crypto
// References:
//  http://www.di-mgt.com.au/rsa_alg.html
//  http://islab.oregonstate.edu/koc/ece575/02Project/Mor/
//  http://people.csail.mit.edu/rivest/Rsapaper.pdf

package main
import "fmt"  // For printf
import "big"  // For the big numbers required for RSA
import "rand" // So we can create random numbers (non-crypto-secure, however)
import "time" // For the nanoseconds to use for seeding the randomiser

// Make a random bignum of size bits, with the highest two and low bit set
func create_random_bignum(bits int) (num *big.Int) {
  num = big.NewInt(3) // Start with 3 so the highest 2 bits are set
  one := big.NewInt(1) // Constant of one
  for num.BitLen() < bits-1 { // Add bits until we're 1 less than we need to be
    num.Lsh(num,1) // num <<= 1 (increase the bitsize by 1)
    if rand.Int() & 1 == 1 { // set the lowest bit randomly
      num.Add(num, one) // num += 1
    }
  }
  num.Lsh(num,1) // num <<= 1 (increase the bitsize by 1)
  num.Add(num,big.NewInt(1)) // num++ - so the lowest bit is set
  return
}

// Create random numbers until it finds a prime
func create_random_prime(bits int) (prime *big.Int) {
  for true {
    prime = create_random_bignum(bits) // Create a random number
    if big.ProbablyPrime(prime, 20) { // Do 20 rabin-miller tests to check if it's prime
      return
    }
  }
  return // This is just here to keep the compiler happy
}

// Test the RSA implementation
func main() {
  println("Test RSA crypto")
  rand.Seed(time.Nanoseconds()) // Initialise the random generator

  // Generate P and Q, two big prime numbers
  println("Generating primes...");
  p := create_random_prime(512)
  q := create_random_prime(512)
  fmt.Printf("Prime p:\r\n %x\r\n", p)
  fmt.Printf("Prime q:\r\n %x\r\n", q)
  
  // Make n (the public key) now: n=p*q
  n := new(big.Int).Mul(p, q)
  fmt.Printf("Public key (n):\r\n %x\r\n", n)
  
  // Public exponent (always 0x10001)
  e := big.NewInt(0x10001)
  fmt.Printf("Exponent (e):\r\n %x\r\n", e)

  // Create phi: (p-1)*(q-1)
  one := big.NewInt(1)
  p_minus_1 := new(big.Int).Sub(p, one)
  q_minus_1 := new(big.Int).Sub(q, one)
  phi := new(big.Int).Mul(p_minus_1, q_minus_1)
  
  // Create the private key - it is the modular multiplicative inverse of e mod phi
  d := new(big.Int).ModInverse(e, phi)
  fmt.Printf("Secret key (d):\r\n %x\r\n", d)

  // Create a message randomly
  m := create_random_bignum(512)
  fmt.Printf("Message (m):\r\n %x\r\n", m)
  
  // Encrypt it: c = m^e mod n
  c := new(big.Int).Exp(m, e, n)
  fmt.Printf("Crypto-text (c):\r\n %x\r\n", c)
  
  // Decrypt it: m = c^d mod n
  a := new(big.Int).Exp(c, d, n)
  fmt.Printf("Message (c):\r\n %x\r\n", a)
}
