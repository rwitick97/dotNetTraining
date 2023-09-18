using System.Security.Cryptography;
using backend.Authorisation;
using backend.Models;
using backend.Repository;

namespace backend.Services
{
    public class AccountService : IAccountService
    {
        private readonly AtmBankingContext _context;
        private readonly IPasswordUtils _utils;
        private readonly IUserRepository _userRepository;
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        public AccountService(AtmBankingContext context, IUserRepository userRepository, IPasswordUtils utils)
        {
            _context = context;
            _userRepository = userRepository;
            _utils = utils;
        }
        public List<int> GetAccountIds(int uid)
        {
            try
            {
                return _context.Accounts.Where(c => c.Userid == uid).ToList().ConvertAll(e => e.Aid);
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }
        public Txn CreateTxnObj(TxnDto t1, Account a, bool isDebit)
        {
            return new Txn()
            {
                Aid = t1.Aid,
                Amount = t1.Amount,
                TxnTime = DateTime.Now,
                Loc = t1.Loc,
                TxnType = t1.TxnType,
                IsDebit = isDebit,
                Remarks = t1.Remarks,
                Currency = a.Currency
            };
        }

        public double CurrencyConverter(string? c1, string? c2, double amt)
        {
#warning currency logic is not yet implemented
            // TODO: currency logic
            return amt;
        }

        private bool CheckPin(Account a, string? pin)
        {
            return _utils.VerifyHashedPasswordV2(a.Pin ?? "", pin ?? "");
        }

        public bool UpdatePin(PinDto p)
        {
            try
            {
                Account? a = _context.Accounts.Find(p.Aid);
                if (a != null && CheckPin(a, p.Pin))
                {
                    a.Pin = _utils.HashPasswordV2(p.Pin.Trim()[0..4], _rng);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CreateAccount(Account a)
        {
            try
            {
                if (_userRepository.GetRegisteredUserById(a.Userid ?? default) != null)
                {
                    a.Balance = 0;
                    a.AccType = a.AccType?.Trim().ToUpper()[0..3];
                    a.IsDeleted = false;
                    _context.Accounts.Add(a);
                    _context.SaveChanges();
                    return true;
                }
                throw new KeyNotFoundException();
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DeleteAccount(int? aid)
        {
            try
            {
                Account? a = _context.Accounts.Find(aid);
                if (a != null)
                {
                    a.IsDeleted = true;
                    _context.SaveChanges();
                    return true;
                }
                throw new KeyNotFoundException();
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool PerformTransaction(TxnDto t1)
        {
            try
            {
                Account? a = _context.Accounts.Find(t1.Aid);
                if (a != null)
                {
                    var t = CreateTxnObj(t1, a, t1.IsDebit);
                    if (t.IsDebit == false)
                    {
                        a.Balance += t.Amount;
                        _context.Txns.Add(t);
                        _context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        if (CheckPin(a, t1.Pin) && t.Amount <= a.Balance)
                        {
                            a.Balance -= t.Amount;
                            _context.Txns.Add(t);
                            _context.SaveChanges();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool PerformTransfer(TxnDto t1)
        {
            try
            {
                Account? a = _context.Accounts.Find(t1.Aid);
                Account? a2 = _context.Accounts.Find(t1.Rec_aid);
                if (a != null && a2 != null)
                {
                    var t = CreateTxnObj(t1, a, true);
                    var t2 = CreateTxnObj(t1, a2, false);
                    if (CheckPin(a, t1.Pin) && t.Amount <= a.Balance)
                    {
                        a.Balance -= t.Amount;
                        a2.Balance += CurrencyConverter(a.Currency, a2.Currency, t1.Amount);
                        _context.Txns.Add(t);
                        _context.Txns.Add(t2);
                        _context.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public List<int> GetTxnIds(int aid)
        {
            try
            {
                Account? a = _context.Accounts.Find(aid);
                if (a != null)
                {
                    return _context.Txns.Where(t => t.Aid == aid).ToList().ConvertAll(t => t.Tid);
                }
                throw new KeyNotFoundException();
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }

        public Txn? GetTxnById(int tid)
        {
            var res = _context.Txns.Find(tid);
            if (res != null)
                res.AidNavigation = null;
            return res;
        }

        public Account? GetAccountById(int aid)
        {
            var res = _context.Accounts.Find(aid);
            if (res != null)
                res.User = null;
            return res;
        }
    }

}