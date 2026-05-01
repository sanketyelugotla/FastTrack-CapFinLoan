FROM python:3.12-slim AS builder

WORKDIR /app

# Install dependencies into a separate location for clean copying
COPY requirements.txt .
RUN pip install --no-cache-dir --prefix=/install -r requirements.txt


FROM python:3.12-slim AS final

WORKDIR /app

# Copy installed packages from builder stage
COPY --from=builder /install /usr/local

# Copy application source code
COPY app/ ./app/

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" capbot && chown -R capbot /app
USER capbot

EXPOSE 8000

# Use 2 workers in production (1 per CPU core is a good baseline)
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000", "--workers", "2"]
