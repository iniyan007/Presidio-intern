const http = require('http');

const options = {
  hostname: 'localhost',
  port: 5082,
  path: '/api/Bookings/00000000-0000-0000-0000-000000000000/reviews',
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-Idempotency-Key': 'test-uuid-123',
    'Authorization': 'Bearer test'
  }
};

const req = http.request(options, res => {
  console.log(`STATUS: ${res.statusCode}`);
  res.on('data', d => {
    process.stdout.write(d);
  });
});

req.on('error', error => {
  console.error(error);
});

req.write(JSON.stringify({
  bookingId: '00000000-0000-0000-0000-000000000000',
  overallRating: 5,
  accommodationRating: 5,
  transportRating: 5,
  foodRating: 5,
  guideRating: 5,
  valueRating: 5,
  comment: 'test',
  mediaFilePaths: []
}));
req.end();
