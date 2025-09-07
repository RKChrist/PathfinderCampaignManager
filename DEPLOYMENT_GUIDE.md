# Pathfinder 2e Campaign Manager - Deployment Guide

## Quick Setup

### Development Environment
1. Install .NET 9 SDK
2. Clone the repository
3. Run `dotnet restore` in the solution root
4. Run `dotnet build` to build the solution
5. Run `dotnet run --project src/Presentation/Server` to start the development server
6. Navigate to `https://localhost:7082` in your browser

### Production Environment
1. Update `appsettings.Production.json` with your database connection string
2. Change the JWT secret key to a secure value
3. Update `AllowedHosts` with your domain
4. Run `dotnet publish -c Release -o ./publish`
5. Deploy the `publish` folder to your web server
6. Configure IIS or Nginx as a reverse proxy

## Configuration Files

### Development Configuration (appsettings.Development.json)
- Uses In-Memory database for quick development
- Detailed logging enabled
- Extended JWT expiration (2 hours)
- Swagger UI enabled
- All features enabled for testing

### Production Configuration (appsettings.Production.json)
- SQL Server database connection
- Optimized logging levels
- Shorter JWT expiration (1 hour)
- Security features enabled
- Performance optimizations
- Rate limiting enabled

## Environment-Specific Settings

### Database
- **Development**: In-Memory database (no setup required)
- **Production**: SQL Server with connection string configuration

### Security
- **Development**: Relaxed security for debugging
- **Production**: HTTPS required, security headers, rate limiting

### Logging
- **Development**: Verbose logging to console
- **Production**: Structured logging to files with retention

### Performance
- **Development**: No caching for immediate updates
- **Production**: Response caching and compression enabled

## Deployment Checklist

### Before Deployment:
- [ ] Update JWT secret key in production config
- [ ] Configure database connection string
- [ ] Update AllowedHosts with your domain
- [ ] Review and adjust user/campaign limits
- [ ] Configure HTTPS certificate
- [ ] Test all major features work

### After Deployment:
- [ ] Verify application starts without errors
- [ ] Test character creation workflow
- [ ] Test campaign creation and management
- [ ] Verify SignalR real-time features work
- [ ] Check validation system functionality
- [ ] Monitor application performance and logs

## Troubleshooting

### Common Issues:
1. **Database Connection**: Verify connection string is correct
2. **SignalR Issues**: Check firewall allows WebSocket connections
3. **HTTPS Problems**: Ensure SSL certificate is valid
4. **Performance**: Monitor memory usage and database queries

### Health Check:
The application includes health check endpoints at `/health` to monitor system status.