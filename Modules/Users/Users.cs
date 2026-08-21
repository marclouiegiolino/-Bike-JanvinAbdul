    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    namespace Api.Modules.Users
    {
        public sealed class User : IEquatable<User>
        {
            public const int USER_NAME_MAX_LENGTH = 100;
            public const int USER_PASS_MAX_LENGTH = 150;

            private int? _userId;
            private long? _roleId;
            private string? _firstName;
            private string? _lastName;
            private string? _phoneNumber;
            private string? _userName;
            private string? _passwordHash;
            private string? _isActive;
            private string? _createdAt;

            public User() { }   

            public User(long roleId, string firstName, string lastName, string phoneNumber, string userName, string passwordHash, string isActive, string createdAt)
            {
                this.RoleId = roleId;
                this.FirstName = firstName;
                this.LastName = lastName;
                this.PhoneNumber = phoneNumber;
                this.UserName = userName;
                this.PasswordHash = passwordHash;
                this.IsActive = isActive;
                this.CreatedAt = createdAt;
            }

            public int UserId
            {
                get => _userId ?? 0;
                set
                {
                    _userId = value;
                }
            }

            public long RoleId
            {
                get => _roleId ?? 0;
                set => _roleId = value;
            }

            /// <summary>
            /// Gets or sets the FirstName.
            /// Maximum length: 50 characters.
            /// </summary>
            [StringLength(50)]
            public string FirstName
            {
                get => _firstName ?? string.Empty;
                set
                {
                    var __s = NormalizeSpaces(value);
                    if (__s.Length > 50)
                        throw new ArgumentException($"FirstName exceeds 50 characters (got {__s.Length}).");
                    _firstName = __s;
                }
            }

            /// <summary>
            /// Gets or sets the LastName.
            /// Maximum length: 50 characters.
            /// </summary>
            [StringLength(50)]
            public string LastName
            {
                get => _lastName ?? string.Empty;
                set
                {
                    var __s = NormalizeSpaces(value);
                    if (__s.Length > 50)
                        throw new ArgumentException($"LastName exceeds 50 characters (got {__s.Length}).");
                    _lastName = __s;
                }
            }

            /// <summary>
            /// Gets or sets the PhoneNumber.
            /// Maximum length: 15 characters.
            /// </summary>
            [StringLength(15)]
            public string PhoneNumber
            {
                get => _phoneNumber ?? string.Empty;
                set
                {
                    var __s = NormalizeSpaces(value);
                    if (__s.Length > 15)
                        throw new ArgumentException($"PhoneNumber exceeds 15 characters (got {__s.Length}).");
                    _phoneNumber = __s;
                }
            }

            /// <summary>
            /// Gets or sets the UserName.
            /// Maximum length: 100 characters.
            /// </summary>
            [StringLength(USER_NAME_MAX_LENGTH)]
            public string UserName
            {
                get => _userName ?? string.Empty;
                set
                {
                    var __s = NormalizeSpaces(value);
                    if (__s.Length > USER_NAME_MAX_LENGTH)
                        throw new ArgumentException($"UserName exceeds USER_NAME_MAX_LENGTH characters (got {__s.Length}).");
                    _userName = __s;
                }
            }

            /// <summary>
            /// Gets or sets the UserPass.
            /// Maximum length: 150 characters.
            /// </summary>
            [StringLength(USER_PASS_MAX_LENGTH)]
            public string PasswordHash
            {
                get => _passwordHash ?? string.Empty;
                set
                {
                    var __s = NormalizeSpaces(value);
                    if (__s.Length > USER_PASS_MAX_LENGTH)
                        throw new ArgumentException($"PasswordHash exceeds USER_PASS_MAX_LENGTH characters (got {__s.Length}).");
                    _passwordHash = __s;
                }
            }

            public string IsActive
            {
                get => _isActive ?? string.Empty;
                set
                {
                    var __s = NormalizeSpaces(value);
                    if (__s.Length > 1)
                        throw new ArgumentException($"IsActive exceeds 1 characters (got {__s.Length}).");
                    _isActive = __s;
                }
            }

            public string CreatedAt
            {
                get => _createdAt ?? string.Empty;
                set
                {
                    var __s = NormalizeSpaces(value);
                    if (__s.Length > 50)
                        throw new ArgumentException($"CreatedAt exceeds 50 characters (got {__s.Length}).");
                    _createdAt = __s;
                }
            }

            public IReadOnlyList<ValidationResult> Validate()
            {
                var ctx = new ValidationContext(this);
                var results = new List<ValidationResult>();
                Validator.TryValidateObject(this, ctx, results, validateAllProperties: true);
                return results;
            }

            public override string ToString() => "User[" + string.Join(", ", new[]
            {
                "UserId=" + UserId,
                "UserName=" + UserName,
                "PasswordHash=" + PasswordHash,
                "RoleId=" + RoleId
            }) + "]";

            public bool Equals(User? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                if (UserId > 0 || other.UserId > 0) return UserId == other.UserId;
                return string.Equals(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(object? obj) => Equals(obj as User);

            public override int GetHashCode()
            {
                if (UserId > 0) return UserId.GetHashCode();
                return StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());
            }

            private static string NormalizeSpaces(string? input)
            {
                if (string.IsNullOrWhiteSpace(input)) return string.Empty;
                var trimmed = input.Trim();
                var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                return string.Join(' ', parts);
            }
        }
    }