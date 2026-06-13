using System.Security.Claims;
using InternManager.Data;
using InternManager.DTO.user;
using InternManager.Model;
using InternManager.Model.info;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class userController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public userController(ApplicationDbContext db)
        {
            _db = db;
        }

        // -- CRUD USER (ADMIN)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Lấy 30 người dùng đầu 
            var users = await _db.Users
                .OrderByDescending(u => u.id)
                .Select(u => new
                {
                    u.id,
                    u.tendangnhap,
                    u.role,
                    u.ngaytao,
                })
                .Take(30)
                .ToListAsync();

            if (!users.Any())
            {
                return Ok(new
                {
                    message = "Hiện tại chưa có người dùng nào trong hệ thống.",
                    user = users
                });
            }

            return Ok(new
            {
                message = "Danh sách 30 TTS mới nhất",
                user = users
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> createUser(userDTO newUserDTO)
        {
            if (await _db.Users.AnyAsync(u => u.tendangnhap == newUserDTO.username))
            {
                return BadRequest("Tên đăng nhập đã tồn tại");
            }

            if (!Enum.IsDefined(typeof(UserRole), newUserDTO.role))
            {
                return BadRequest("Quyền (Role) không hợp lệ. Chỉ chấp nhận các giá trị: Admin, Leader, TTS.");
            }

            var newUser = new users
            {
                tendangnhap = newUserDTO.username,
                matkhau = newUserDTO.password,
                role = newUserDTO.role
            };
            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Đăng ký thành công cho người dùng",
                tendangnhap = newUserDTO.username
            });
        }

        // Tạo user theo kiểu dạng danh sách 
        // có thể áp dụng cho việc đẩy các file excel lên
        [HttpPost("create-list")]   
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateUsers(List<userDTO> newUsersDTO)
        {
            if (newUsersDTO == null || !newUsersDTO.Any())
            {
                return BadRequest("Danh sách người dùng không được để trống.");
            }

            var complete = new List<string>();
            var failexit = new List<string>();

            var localProcessedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dto in newUsersDTO)
            {
                if (localProcessedUsernames.Contains(dto.username))
                {
                    failexit.Add($"{dto.username} (Trùng lặp trong danh sách gửi lên)");
                    continue;
                }

                if (await _db.Users.AnyAsync(u => u.tendangnhap == dto.username))
                {
                    failexit.Add($"{dto.username} (Đã tồn tại dưới Database)");
                    continue;
                }

                localProcessedUsernames.Add(dto.username);

                var newUser = new users
                {
                    tendangnhap = dto.username,
                    matkhau = dto.password,
                    role = dto.role,
                    status = dto.status,
                    ngaytao = dto.create_at
                };

                _db.Users.Add(newUser);
                complete.Add(dto.username);
            }

            if (complete.Any())
            {
                await _db.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"Xử lý xong! Tạo thành công {complete.Count} user, thất bại {failexit.Count} user do trùng tên.",
                userThanhCong = complete,
                userBiTrung = failexit
            });
        }

        [HttpPut]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser(int userID, userDTO updateUserDTO)
        {
            var UserID = await _db.Users.FirstOrDefaultAsync(u => u.id == userID);
            if (UserID == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy người dùng này."
                });
            }


            if (UserID.tendangnhap != updateUserDTO.username)
            {
                var isNameTaken = await _db.Users.AnyAsync(u => u.tendangnhap == updateUserDTO.username);
                if (isNameTaken)
                {
                    return BadRequest("Tên đăng nhập đã tồn tại.");
                }
            }

            UserID.tendangnhap = updateUserDTO.username;
            UserID.matkhau = updateUserDTO.password;
            UserID.role = updateUserDTO.role;
            UserID.status = updateUserDTO.status;

            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Cập nhật thành công thông tin cho ",
                tendangnhap = updateUserDTO.username
            });
        }

        [HttpGet("search")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SearchUser(string username)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(u => u.tendangnhap.Contains(username));
            }

            var result = await query
                .Select(u => new
                {
                    u.id,
                    u.tendangnhap,
                    u.role,
                    u.ngaytao,
                    u.status
                })
                .ToListAsync();

            if (result.Count == 0)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy người dùng nào có tên chứa chữ '{username}'."
                });
            }

            return Ok(new
            {
                message = $"Tìm thành công người dùng {username}.",
                users = result
            });
        }

        [HttpGet("info")]
        [Authorize(Roles = "admin, tts")]
        public async Task<IActionResult> GetInfoUsers(int UserID)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không tìm thấy danh tính từ Cookie, hệ thống mới đá ra
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized(new { message = "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn." });
            }

            UserID = int.Parse(userIdStr);

            var userInfo = await _db.Users_Infos
                .Where(ui => ui.User_ID == UserID)
                .Select(ui => new
                {
                    ui.id,
                    ui.User_ID,
                    username = ui.Users.tendangnhap,
                    ui.hoten,
                    ui.nganh_hoc,
                    ui.vi_tri,
                    ui.mssv,
                    ui.truong,
                    ui.ngay_bat_dau,
                    ui.thoi_gian_thuctap,
                    ui.email_truong,
                    ui.email_ca_nhan,
                    ui.gioi_tinh,
                    ui.gpa,
                    ui.trinh_do_tieng_anh,
                    ui.gioi_thieu,
                    ui.dia_chi,
                    ui.fb_url,
                    ui.sdt,
                    ui.cccd,
                    ui.ngay_cap_cccd,
                    ui.noi_cap_cccd,
                    ui.cv
                })
                .FirstOrDefaultAsync();


            if (userInfo == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy thông tin cá nhân của người dùng có ID = {UserID}."
                });
            }

            return Ok(new
            {
                message = $"Lấy thông tin cá nhân của người dùng có ID = {UserID} thành công.",
                data = userInfo
            });
        }

        [HttpPost("info")]
        [Authorize(Roles = "admin, tts")]
        public async Task<IActionResult> createUserInfo(userInfoDTO newUserInfoDTO)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("Phiên làm việc không hợp lệ.");
            int userID = int.Parse(userIdStr);

            var userExists = await _db.Users.AnyAsync(u => u.id == userID);
            if (!userExists)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy tài khoản (User) có ID = {userID}."
                });
            }


            if (string.IsNullOrEmpty(newUserInfoDTO.email_school) && string.IsNullOrEmpty(newUserInfoDTO.email_personal))
            {
                ModelState.AddModelError("EmailError", "Bạn phải điền ít nhất 1 trong 2 loại email (Email trường hoặc Email cá nhân).");
                return BadRequest(ModelState);
            }


            var infoExists = await _db.Users_Infos.AnyAsync(ui => ui.User_ID == userID);
            if (infoExists)
            {
                return BadRequest(new
                {
                    message = "Tài khoản này đã có thông tin cá nhân trên hệ thống. Vui lòng sử dụng chức năng cập nhật."
                });
            }

            if (await _db.Users_Infos.AnyAsync(ui => ui.email_truong == newUserInfoDTO.email_school))
            {
                return BadRequest("Email này đã tồn tại trong cơ sở dữ liệu");
            }

            if (await _db.Users_Infos.AnyAsync(ui => ui.email_ca_nhan == newUserInfoDTO.email_personal))
            {
                return BadRequest("Email này đã tồn tại trong cơ sở dữ liệu");
            }

            if (await _db.Users_Infos.AnyAsync(ui => ui.sdt == newUserInfoDTO.sdt))
            {
                return BadRequest("Số điện thoại này đã tồn tại trong cơ sở dữ liệu");
            }

            if (await _db.Users_Infos.AnyAsync(ui => ui.cccd == newUserInfoDTO.cccd))
            {
                return BadRequest("Mã căn cước công dân này đã tồn tại trong cơ sở dữ liệu");
            }

            var newUserInfo = new users_info
            {
                User_ID = userID,
                hoten = newUserInfoDTO.fullname,
                nganh_hoc = newUserInfoDTO.study,
                vi_tri = newUserInfoDTO.postion,
                mssv = newUserInfoDTO.studentID,
                truong = newUserInfoDTO.school,
                ngay_bat_dau = newUserInfoDTO.start_intern,
                thoi_gian_thuctap = newUserInfoDTO.duration_intern,
                email_truong = newUserInfoDTO.email_school,
                email_ca_nhan = newUserInfoDTO.email_personal,
                gioi_tinh = newUserInfoDTO.gioi_tinh,
                gpa = newUserInfoDTO.gpa,
                trinh_do_tieng_anh = newUserInfoDTO.english_level,
                gioi_thieu = newUserInfoDTO.description,
                dia_chi = newUserInfoDTO.location,
                fb_url = newUserInfoDTO.fb_url,
                sdt = newUserInfoDTO.sdt,
                cccd = newUserInfoDTO.cccd,
                ngay_cap_cccd = newUserInfoDTO.cccd_create,
                noi_cap_cccd = newUserInfoDTO.cccd_location,
                cv = newUserInfoDTO.cv
            };
            _db.Users_Infos.Add(newUserInfo);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo thông tin thành công cho người dùng",
                tendangnhap = newUserInfoDTO.fullname
            });
        }

        [HttpPut("info")]
        [Authorize(Roles = "admin, tts")]
        public async Task<IActionResult> UpdateUserInfo(int userID, userInfoDTO updateUserInfoDTO)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("Phiên làm việc không hợp lệ.");
            userID = int.Parse(userIdStr);

            var UserID = await _db.Users_Infos.FirstOrDefaultAsync(ui => ui.User_ID == userID);
            if (UserID == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy người dùng này."
                });
            }

            if (UserID.email_truong != updateUserInfoDTO.email_school)
            {
                var isEmailSchool = await _db.Users_Infos.AnyAsync(ui => ui.email_truong == updateUserInfoDTO.email_school);
                if (isEmailSchool)
                {
                    return BadRequest("Email trường này đã tồn tại.");
                }
            }

            if (UserID.email_ca_nhan != updateUserInfoDTO.email_personal)
            {
                var isEmailPersonal = await _db.Users_Infos.AnyAsync(ui => ui.email_ca_nhan == updateUserInfoDTO.email_personal);
                if (isEmailPersonal)
                {
                    return BadRequest("Email cá nhân này đã tồn tại.");
                }
            }

            if (UserID.sdt != updateUserInfoDTO.sdt)
            {
                var isSDT = await _db.Users_Infos.AnyAsync(ui => ui.sdt == updateUserInfoDTO.sdt);
                if (isSDT)
                {
                    return BadRequest("SDT này đã tồn tại.");
                }
            }

            if (UserID.cccd != updateUserInfoDTO.cccd)
            {
                var isSDT = await _db.Users_Infos.AnyAsync(ui => ui.cccd == updateUserInfoDTO.cccd);
                if (isSDT)
                {
                    return BadRequest("Căn cước công dân này đã tồn tại.");
                }
            }

            UserID.hoten = updateUserInfoDTO.fullname;
            UserID.nganh_hoc = updateUserInfoDTO.study;
            UserID.vi_tri = updateUserInfoDTO.postion;
            UserID.mssv = updateUserInfoDTO.studentID;
            UserID.truong = updateUserInfoDTO.school;
            UserID.ngay_bat_dau = updateUserInfoDTO.start_intern;
            UserID.thoi_gian_thuctap = updateUserInfoDTO.duration_intern;
            UserID.email_truong = updateUserInfoDTO.email_school;
            UserID.email_ca_nhan = updateUserInfoDTO.email_personal;
            UserID.gioi_tinh = updateUserInfoDTO.gioi_tinh;
            UserID.gpa = updateUserInfoDTO.gpa;
            UserID.trinh_do_tieng_anh = updateUserInfoDTO.english_level;
            UserID.gioi_thieu = updateUserInfoDTO.description;
            UserID.dia_chi = updateUserInfoDTO.location;
            UserID.fb_url = updateUserInfoDTO.fb_url;
            UserID.sdt = updateUserInfoDTO.sdt;
            UserID.cccd = updateUserInfoDTO.cccd;
            UserID.ngay_cap_cccd = updateUserInfoDTO.cccd_create;
            UserID.noi_cap_cccd = updateUserInfoDTO.cccd_location;
            UserID.cv = updateUserInfoDTO.cv;

            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Cập nhật thành công thông tin cho ",
                hoten = updateUserInfoDTO.fullname
            });
        }

        // Chỉ dùng cho admin hoặc leader lấy danh sách 
        // thông tin thực tập sinh
        [HttpGet("info/list-all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllInfoUsers()
        {
            // Lấy 30 thông tin chi tiết của các TTS mới nhất
            var usersInfoList = await _db.Users_Infos
                .Include(ui => ui.Users)
                .OrderByDescending(ui => ui.id)
                .Select(ui => new
                {
                    ui.id,
                    ui.User_ID,
                    // Lấy dữ liệu từ bảng users
                    username = ui.Users.tendangnhap,
                    // Lấy dữ liệu chi tiết từ bảng users_info
                    ui.hoten,
                    ui.nganh_hoc,
                    ui.vi_tri,
                    ui.mssv,
                    ui.truong,
                    ui.ngay_bat_dau,
                    ui.thoi_gian_thuctap,
                    ui.email_truong,
                    ui.email_ca_nhan,
                    ui.gioi_tinh,
                    ui.gpa,
                    ui.trinh_do_tieng_anh,
                    ui.gioi_thieu,
                    ui.dia_chi,
                    ui.fb_url,
                    ui.sdt,
                    ui.cccd,
                    ui.ngay_cap_cccd,
                    ui.noi_cap_cccd,
                    ui.cv
                })
                .Take(30)
                .ToListAsync();

            // Kiểm tra nếu không có dữ liệu
            if (!usersInfoList.Any())
            {
                return Ok(new
                {
                    message = "Hiện tại chưa có thông tin người dùng nào trong hệ thống.",
                    data = usersInfoList
                });
            }

            return Ok(new
            {
                message = "Danh sách thông tin chi tiết 30 TTS mới nhất",
                data = usersInfoList
            });
        }
        [HttpGet("info/search")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SearchUserInfo(string? hoten)
        {
            var query = _db.Users_Infos.Include(ui => ui.Users).AsQueryable();

            if (!string.IsNullOrEmpty(hoten))
            {
                query = query.Where(ui => ui.hoten.Contains(hoten));
            }

            var result = await query
                .Select(ui => new
                {
                    // users
                    TenDangNhap = ui.Users.tendangnhap,
                    // user_info         
                    ui.User_ID,
                    ui.hoten,
                    ui.nganh_hoc,
                    ui.vi_tri,
                    ui.mssv,
                    ui.truong,
                    ui.ngay_bat_dau,
                    ui.thoi_gian_thuctap,
                    ui.email_truong,
                    ui.email_ca_nhan,
                    ui.gioi_tinh,
                    ui.gpa,
                    ui.trinh_do_tieng_anh,
                    ui.gioi_thieu,
                    ui.dia_chi,
                    ui.fb_url,
                    ui.sdt,
                    ui.cccd,
                    ui.ngay_cap_cccd,
                    ui.noi_cap_cccd,
                    ui.cv
                })
                .ToListAsync();

            if (result.Count == 0)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy người dùng nào có họ tên '{hoten}'."
                });
            }

            return Ok(new
            {
                message = $"Tìm thấy {result.Count} kết quả phù hợp với họ tên '{hoten}'.",
                users = result
            });
        }
    }
}
