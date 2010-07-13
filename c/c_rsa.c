// Simple, thoroughly commented implementation of RSA in C
// Chris Hulbert - chris.hulbert@gmail.com - http://splinter.com.au/blog

#include <stdio.h>
#include <string.h>

// Some useful data structures
typedef unsigned char byte;
#define BIGNUM_MAX_BYTES 1024 // The max size of a bignum, with 1k we should be able to handle RSA-2048 bits easily
typedef struct bignum {
  byte bytes[BIGNUM_MAX_BYTES]; // Stored least-significant first
} bignum;

// Some sample data
char *p_str = "dd755ca44f2f399a845690ef8507365befef9505fbf416cb0b306bd13221a00368e8bd45f7357d2686b8437816da326dc40b7c756d2407bb9a8c8a3fb2b8e79d";
char *q_str = "e083f0abbd0bee477bc86aa12077b82b5f7a035ac614dd494fa55a57e03deea0527f54e31e715374b3dd992ea9f80bb94b7e3b2b4e02e8901af79e688c1c7483";
char *e_str = "010001";
char *n_str = "c238d450c526bb2014b1489505540eb8330c7e01ed7ac4a7d9a52423025f9bdd5eb42b2103b6a069e43678bef68fa67703c304c590c6629bd455f4d8c0a145599df37bdefa19b52532937a2ccc22fb36f73c6dad819bc01e1326028fab37a052e0efae05e437573f2254a5ea4a43d1f3dbec2b22bf24fc6dddd0443f6ebda957";
char *d_str = "305bf211826558666e808deffcf9a7089a3d5c0aa2d4d4ae6e74be00b19098c08fda107b11efa1157cab4b7950ef07a5ce9bfa4e2ef4168d725b4cb1c394e42d332999fa20a42f4c31fdeba079c6931a11915f66d2b47c75571d334ce075bc417df8bc0848ae97b7abf6472ab7c83de2da691115a864d32496200d26a1d91791";
char *message = "This is going to be embarrasing if it doesn't work!";
char *c_str = "68c1a28435c90c20e3e0111302f97222c875215ce37178cdca30fbd90fceafaa7aa90c5d0dee2290a3b4cf944a177175acd5cb29cb03869bce2b4f93357cb94b08f8f1f08f793f9a7015338be19ff6b9301aa144665ffe0f7749885d3c3a51f8627d1e26ad629525eee59da7d5c69fe2926b6fb51ded336b6033a203d1ef5bc3";

// Some bignum functions

// Parse two chars as hex
int parse_hex_byte(char *input, int offset) {
  char temp[3];
  temp[0]=input[offset];
  temp[1]=input[offset+1];
  temp[2]=0;
  int val;
  sscanf(temp,"%x",&val);
  return val;
}

// Convert an even-digits string eg "010001" to a bignum
void string_to_bignum(char *input, bignum *output) {
  memset(output,0,sizeof(bignum)); // Clear the bignum first
  int bytes = strlen(input)/2; // Figure out how many bytes there'll be
  for (int i=0;i<bytes;i++) {
    output->bytes[i] = parse_hex_byte(input,(bytes-i-1)*2);
  }
}

// Find the number of significant bytes in the bignum
int bignum_length(bignum *val) {
  int len=1;
  // Look for the highest nonzero byte 
  for (int i=0;i<BIGNUM_MAX_BYTES;i++) {
    if (val->bytes[i]) {
      len = i+1;
    }
  }
  return len;
}

// Print a bignum to screen
void print_bignum(bignum *val, char *message) {
  printf ("%s:\r\n ", message);
  for (int i=bignum_length(val)-1;i>=0;i--) {
    printf ("%02x",val->bytes[i]);
  }
  printf ("\r\n");
}

// Add a + b, storing the result in a 
void bignum_add(bignum *a, bignum *b) {
  int carry=0;
  for (int i=0;i<BIGNUM_MAX_BYTES;i++) {
    int newval = a->bytes[i] + b->bytes[i] + carry;
    a->bytes[i] = newval % 256;
    carry = newval / 256;
  }
}

// Do a - b, storing the result in a, returning nonzero if b is bigger
int bignum_subtract(bignum *a, bignum *b) {
  int borrow=0;
  for (int i=0;i<BIGNUM_MAX_BYTES;i++) {
    int newval = a->bytes[i] - b->bytes[i] - borrow;
    if (newval<0) {
      newval+=256;
      borrow=1;
    }
    else {
      borrow=0;
    }
    a->bytes[i] = newval;
  }
  return borrow;
}

// Returns nonzero if a >= b
int bignum_gte(bignum *a, bignum *b) {
  // First check if they're equal
  if (!memcmp(a,b,sizeof(bignum))) return 1;
  // Now do a subtract to find which is bigger
  int borrow=0;
  for (int i=0;i<BIGNUM_MAX_BYTES;i++) {
    int newval = b->bytes[i] - a->bytes[i] - borrow;
    if (newval<0) {
      borrow=1;
    }
    else {
      borrow=0;
    }
  }
  return borrow;
}

// Multiply a bignum by 1 digit
void bignum_mult1(bignum *a, bignum *out, int mult) {
  int carry=0;
  for (int i=0;i<BIGNUM_MAX_BYTES;i++) {
    int newval = a->bytes[i] * mult + carry;
    out->bytes[i] = newval % 256;
    carry = newval / 256;
  }
}

// Shift left by whole digits, eg make a bignum even bigger
void bignum_shiftleft(bignum *a, int digits) {
  if (digits==0) return;
  memcpy(a->bytes+digits,a->bytes,BIGNUM_MAX_BYTES-digits); // Move the digits
  memset(a->bytes,0,digits); // zero out the new bottom digits
}

// Multiply two bignums, storing the result in a
void bignum_mult(bignum *a, bignum *b, bignum *out) {
  bignum temp; // This is used to store the multiplication by each digit
  memset(&temp,0,sizeof(bignum)); // Clear the bignum first
  memset(out ,0,sizeof(bignum)); // Clear the bignum first
  for (int i=0;i<BIGNUM_MAX_BYTES;i++) {
    if (b->bytes[i]) { // Save time by skipping multiplying by zero columns
      bignum_mult1(a,&temp,b->bytes[i]); // temp = a * single-digit-from-b
      bignum_shiftleft(&temp,i); // temp is shifted to line up with the column we're using from b
      bignum_add(out,&temp); // Add temp to the running total
    }
  }
}

// Shifts a bignum right one bit (making it smaller)
void bignum_shiftright_onebit(bignum *a) {
  for (int i=0;i<BIGNUM_MAX_BYTES-1;i++) {
    a->bytes[i] = (a->bytes[i]>>1) + ((a->bytes[i+1]&1)<<7); // Shift this one right, plug borrow a bit from the next
  }
  a->bytes[BIGNUM_MAX_BYTES-1] >>= 1; // Sort out the last byte (it can't borrow a bit)
}

// Do a/b, and store the remainder in out
void bignum_mod(bignum *a, bignum *b, bignum *out) {
  memcpy(out,a,sizeof(bignum)); // Start off with out=a, and whittle it down
  // Get the lengths
  int len_a = bignum_length(a);
  int len_b = bignum_length(b);
  if (len_b>len_a) return; // Simple case: since b is bigger, a is already the modulus
  // Start by shifting b so it's bigger than a
  bignum shifted;
  int byte_shifts = len_a-len_b+1;
  memcpy(&shifted,b,sizeof(bignum)); // Shifted is b, shifted to all sizes
  bignum_shiftleft(&shifted,byte_shifts); // Now b is bigger than a
  // Now do a series of bit shifts on B, subtracting it from A each time
  for (int i=0;i<byte_shifts*8;i++) {
    bignum_shiftright_onebit(&shifted);
    if (bignum_gte(out,&shifted))
      bignum_subtract(out,&shifted);
  }
}

// Test it
int main(void) {
  bignum p, q, n;
  string_to_bignum(p_str, &p);
  string_to_bignum(q_str, &q);
  string_to_bignum(n_str, &n);
  print_bignum(&p, "P prime");
  print_bignum(&q, "Q prime");
  print_bignum(&n, "N modulus");
  
  // Test %
  bignum mod;
  bignum_mod(&n, &p, &mod);
  print_bignum(&mod, "N % P");
}
