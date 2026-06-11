# =========================================================
# Web.Dockerfile — Multi-stage build cho DineFlow.Web (MVC)
#
# [KIẾN THỨC] DineFlow.Web chỉ reference DineFlow.Application
# → Không cần copy Infrastructure/API source code
# → Nhẹ hơn, build nhanh hơn
#
# [KIẾN THỨC] ApiBaseUrl inject lúc runtime qua Environment Variable:
# → docker run -e ApiBaseUrl=http://api:8080 dineflow-web
# → Không hardcode URL trong image → image reusable qua mọi môi trường
# =========================================================

# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Layer cache: copy project files trước
COPY DineFlow.slnx ./
COPY src/DineFlow.Domain/DineFlow.Domain.csproj               src/DineFlow.Domain/
COPY src/DineFlow.Application/DineFlow.Application.csproj     src/DineFlow.Application/
COPY src/DineFlow.Web/DineFlow.Web.csproj                     src/DineFlow.Web/

# Restore chỉ dependencies của Web (không cần Infrastructure)
RUN dotnet restore src/DineFlow.Web/DineFlow.Web.csproj

# Copy source
COPY src/DineFlow.Domain/     src/DineFlow.Domain/
COPY src/DineFlow.Application/ src/DineFlow.Application/
COPY src/DineFlow.Web/        src/DineFlow.Web/

# Publish
RUN dotnet publish src/DineFlow.Web/DineFlow.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# [KIẾN THỨC] ApiBaseUrl: được override bởi docker-compose hoặc k8s env vars
# Default trỏ đến service name "api" trong Docker network
ENV ApiBaseUrl=http://api:8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "DineFlow.Web.dll"]
