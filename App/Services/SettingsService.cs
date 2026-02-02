using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace App.Services
{
    public partial class SettingsService<T> : IDisposable where T : class, new()
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly CancellationTokenSource _cts = new();
        
        private T _value;

        public event Action<T>? SettingsChanged;
        
        public T Value => _value;

        public SettingsService(string fileName = "settings.json")
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            _value = new T();
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            _value = Load();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            await LoadAsync(cancellationToken);
        }

        private T Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var content = File.ReadAllText(_filePath);

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var settings = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                        if (settings != null)
                        {
                            Debug.WriteLine($"SettingsService: Settings loaded from {_filePath}");
                            return settings;
                        }
                    }
                }

                // Create default settings
                var defaultSettings = new T();
                var json = JsonSerializer.Serialize(defaultSettings, _jsonOptions);
                File.WriteAllText(_filePath, json);

                Debug.WriteLine($"SettingsService: Created default settings file at {_filePath}");
                return defaultSettings;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsService: Error loading settings: {ex.Message}");
                return new T();
            }
        }

        public async Task<T> LoadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (File.Exists(_filePath))
                {
                    var content = await File.ReadAllTextAsync(_filePath, cancellationToken);
                    
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        _value = JsonSerializer.Deserialize<T>(content, _jsonOptions) ?? new T();
                    }
                    else
                    {
                        _value = new T();
                    }
                }
                else
                {
                    _value = new T();
                    
                    var json = JsonSerializer.Serialize(_value, _jsonOptions);
                    await File.WriteAllTextAsync(_filePath, json, cancellationToken);
                    
                    Debug.WriteLine($"SettingsService: Created default settings file at {_filePath}");
                }

                return _value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsService: Error loading settings: {ex.Message}");
                _value = new T();
                return _value;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync(T settings, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            Debug.WriteLine("SettingsService: SaveAsync - Waiting for lock...");
            await _lock.WaitAsync(cancellationToken);
            Debug.WriteLine("SettingsService: SaveAsync - Lock acquired");
            
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json, cancellationToken);
                Debug.WriteLine($"SettingsService: Settings saved to {_filePath}");

                _value = settings;
                
                Debug.WriteLine($"SettingsService: Invoking SettingsChanged event, Subscribers: {SettingsChanged?.GetInvocationList().Length ?? 0}");
                SettingsChanged?.Invoke(_value);
                Debug.WriteLine("SettingsService: SettingsChanged event invoked");
            }
            finally
            {
                _lock.Release();
                Debug.WriteLine("SettingsService: SaveAsync - Lock released");
            }
        }

        public async Task UpdateAsync(Action<T> updateAction, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            await _lock.WaitAsync(cancellationToken);
            try
            {
                updateAction(_value);
                
                var json = JsonSerializer.Serialize(_value, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json, cancellationToken);
                
                SettingsChanged?.Invoke(_value);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _lock.Dispose();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
