using BCryptNet = BCrypt.Net.BCrypt;

var password = "Admin@123";
var hashed = BCryptNet.EnhancedHashPassword(password);

Console.WriteLine(hashed);
