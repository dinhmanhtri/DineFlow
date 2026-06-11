# =========================================================
# API.Dockerfile — Multi-stage build cho DineFlow.API
#
# [KIẾN THỨC] Multi-stage build:
# Stage 1 (build):   SDK image (~800MB) → compile, publish
# Stage 2 (runtime): ASP.NET Runtime image (~250MB) → chỉ chứa output
# → Image cuối nhỏ hơn nhiều (không có compiler, source code)
#
# [KIẾN THỨC] Layer caching:
# COPY *.csproj TRƯỚC khi COPY source code
# → Nếu chỉ thay đổi code (không đổi packages) → layer restore được cache
# → Build nhanh hơn đáng kể
# =========================================================

# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + project files trước để cache restore layer
COPY DineFlow.slnx ./
COPY src/DineFlow.Domain/DineFlow.Domain.csproj               src/DineFlow.Domain/
COPY src/DineFlow.Application/DineFlow.Application.csproj     src/DineFlow.Application/
COPY src/DineFlow.Infrastructure/DineFlow.Infrastructure.csproj src/DineFlow.Infrastructure/
COPY src/DineFlow.API/DineFlow.API.csproj                     src/DineFlow.API/
COPY src/DineFlow.Web/DineFlow.Web.csproj                     src/DineFlow.Web/

# Restore packages (layer này được cache nếu .csproj không đổi)
RUN dotnet restore src/DineFlow.API/DineFlow.API.csproj

# Copy toàn bộ source code
COPY src/ src/

# Publish API (Release mode, no self-contained → dùng runtime image)
RUN dotnet publish src/DineFlow.API/DineFlow.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# [KIẾN THỨC] Non-root user: chạy app với user ít quyền hơn → security best practice
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy chỉ publish output từ stage 1
COPY --from=build /app/publish .

# Port: ASP.NET Core dùng 8080 (HTTP) trong container theo convention mới
EXPOSE 8080

# [KIẾN THỨC] ASPNETCORE_URLS: override launchSettings.json
# → trong container không dùng launchSettings.json
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check trong container
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "DineFlow.API.dll"]
