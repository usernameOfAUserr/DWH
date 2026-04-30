<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { ImportService } from "@/Services/ImportService.ts";
import type { ImportStatus } from "@/Models/ImportStatus.ts";

const status = ref<ImportStatus>({
  isRunning: false,
  progress: 0,
  message: "Bereit für den Import",
});

const isStarting = ref(false);

let intervalId: number | null = null;

const normalizedProgress = computed(() => {
  return Math.max(0, Math.min(100, status.value.progress));
});

const progressWidth = computed(() => `${normalizedProgress.value}%`);

const progressLabel = computed(() => {
  if (status.value.isRunning)
  {
    return `${normalizedProgress.value}%`;
  }

  if (normalizedProgress.value >= 100)
  {
    return "Abgeschlossen";
  }

  return "Bereit";
});

const statusTitle = computed(() => {
  if (isStarting.value)
  {
    return "Import wird gestartet";
  }

  if (status.value.isRunning)
  {
    return "Analyse läuft";
  }

  if (normalizedProgress.value >= 100)
  {
    return "Import abgeschlossen";
  }

  return "Analyse bereit";
});

const statusText = computed(() => {
  if (status.value.message.trim().length > 0)
  {
    return status.value.message;
  }

  if (status.value.isRunning)
  {
    return "Die Daten werden verarbeitet und in Echtzeit analysiert.";
  }

  if (normalizedProgress.value >= 100)
  {
    return "Die Analyse wurde erfolgreich beendet.";
  }

  return "Starte den Import, um die Analyse zu beginnen.";
});

const modeLabel = computed(() => {
  if (isStarting.value)
  {
    return "Initialisierung";
  }

  if (status.value.isRunning)
  {
    return "Verarbeitung";
  }

  if (normalizedProgress.value >= 100)
  {
    return "Fertig";
  }

  return "Bereit";
});

const heroStateText = computed(() => {
  if (isStarting.value)
  {
    return "Booting pipeline";
  }

  if (status.value.isRunning)
  {
    return "Realtime processing";
  }

  if (normalizedProgress.value >= 100)
  {
    return "Completed";
  }

  return "Ready to launch";
});

const particles = computed(() => {
  return Array.from({ length: 16 }, (_, index) => ({
    id: index,
    left: `${6 + ((index * 6.1) % 88)}%`,
    top: `${8 + ((index * 5.7) % 78)}%`,
    delay: `${(index % 8) * 0.55}s`,
    duration: `${7 + (index % 5)}s`,
    size: `${8 + (index % 4) * 6}px`,
  }));
});

function shouldStopPolling(importStatus: ImportStatus): boolean
{
  return !importStatus.isRunning || importStatus.progress >= 100;
}

function stopPolling(): void
{
  if (intervalId === null)
  {
    return;
  }

  window.clearInterval(intervalId);
  intervalId = null;
}

async function loadStatus(): Promise<ImportStatus | null>
{
  try
  {
    var result = await ImportService.getStatus();
    status.value = result;
    return result;
  }
  catch
  {
    return null;
  }
}

function startPolling(): void
{
  stopPolling();

  intervalId = window.setInterval(async () => {
    var result = await loadStatus();

    if (result === null)
    {
      return;
    }

    if (shouldStopPolling(result))
    {
      stopPolling();
    }
  }, 2000);
}

async function startImport(): Promise<void>
{
  if (isStarting.value || status.value.isRunning)
  {
    return;
  }

  isStarting.value = true;
  stopPolling();

  try
  {
    ImportService.startImport();

    var result = await loadStatus();

    if (result !== null && !shouldStopPolling(result))
    {
      startPolling();
    }
  }
  finally
  {
    isStarting.value = false;
  }
}

onMounted(async () => {
  var result = await loadStatus();

  if (result !== null && result.isRunning && result.progress < 100)
  {
    startPolling();
  }
});

onBeforeUnmount(() => {
  stopPolling();
});
</script>

<template>
  <div
      class="import-page"
      :class="{
      running: status.isRunning,
      starting: isStarting,
      completed: status.progress >= 100 && !status.isRunning,
    }"
  >
    <div class="ambient ambient-1"></div>
    <div class="ambient ambient-2"></div>
    <div class="ambient ambient-3"></div>
    <div class="noise-layer"></div>
    <div class="grid-layer"></div>
    <div class="scan-line"></div>

    <div class="particles-layer">
      <span
          v-for="particle in particles"
          :key="particle.id"
          class="particle"
          :style="{
          left: particle.left,
          top: particle.top,
          width: particle.size,
          height: particle.size,
          animationDelay: particle.delay,
          animationDuration: particle.duration,
        }"
      ></span>
    </div>

    <section class="hero-card">
      <div class="hero-topline">
        <div class="hero-badge">
          <span class="badge-pulse"></span>
          Analysezentrum
        </div>

        <div class="hero-chip">
          <span class="hero-chip-dot"></span>
          {{ heroStateText }}
        </div>
      </div>

      <div class="hero-content">
        <div class="hero-main">
          <h1 class="hero-title">
            Datenimport
            <span class="title-gradient">& Analyse</span>
          </h1>

          <p class="hero-description">
            Starte den Importprozess und verfolge live, wie deine Daten verarbeitet, strukturiert und für die Auswertung vorbereitet werden.
          </p>

          <div class="hero-actions">
            <button
                class="start-button"
                :disabled="isStarting || status.isRunning"
                @click="startImport"
            >
              <span class="button-background"></span>
              <span class="button-ring"></span>
              <span class="button-shine"></span>
              <span class="button-text">
                {{ isStarting ? "Wird gestartet..." : status.isRunning ? "Analyse läuft" : "Import starten" }}
              </span>
            </button>
          </div>
        </div>

        <div class="hero-visual">
          <div class="radar-card">
            <div class="radar-orbit orbit-1"></div>
            <div class="radar-orbit orbit-2"></div>
            <div class="radar-orbit orbit-3"></div>
            <div class="radar-center"></div>
            <div class="radar-sweep"></div>

            <div class="radar-stats">
              <div class="radar-stat">
                <span>Progress</span>
                <strong>{{ normalizedProgress }}%</strong>
              </div>

              <div class="radar-stat">
                <span>Mode</span>
                <strong>{{ modeLabel }}</strong>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="dashboard-grid">
      <article class="panel status-panel">
        <div class="panel-glow"></div>

        <div class="panel-header">
          <div>
            <div class="panel-label">
              Status
            </div>

            <div class="panel-title">
              {{ statusTitle }}
            </div>
          </div>

          <div
              class="status-indicator"
              :class="{ active: status.isRunning }"
          >
            <span class="status-dot"></span>
            <span>{{ status.isRunning ? "Aktiv" : "Inaktiv" }}</span>
          </div>
        </div>

        <p class="status-message">
          {{ statusText }}
        </p>

        <div class="progress-section">
          <div class="progress-meta">
            <span>Fortschritt</span>
            <strong>{{ progressLabel }}</strong>
          </div>

          <div class="progress-track">
            <div class="progress-track-glow"></div>

            <div
                class="progress-fill"
                :style="{ width: progressWidth }"
            >
              <div class="progress-spark progress-spark-1"></div>
              <div class="progress-spark progress-spark-2"></div>
              <div class="progress-spark progress-spark-3"></div>
            </div>
          </div>
        </div>

        <div class="timeline-row">
          <div class="timeline-chip" :class="{ active: isStarting || status.isRunning || normalizedProgress > 0 }">
            Initialisiert
          </div>

          <div class="timeline-chip" :class="{ active: status.isRunning || normalizedProgress >= 45 }">
            Verarbeitung
          </div>

          <div class="timeline-chip" :class="{ active: normalizedProgress >= 100 }">
            Abgeschlossen
          </div>
        </div>
      </article>

      <article class="panel metrics-panel">
        <div class="panel-glow panel-glow-secondary"></div>

        <div class="panel-label">
          Überblick
        </div>

        <div class="metrics-grid">
          <div class="metric-card floating-card">
            <span class="metric-title">Fortschritt</span>
            <strong class="metric-value">{{ normalizedProgress }}%</strong>
            <span class="metric-backdrop-number">{{ normalizedProgress }}</span>
          </div>

          <div class="metric-card floating-card delayed">
            <span class="metric-title">Modus</span>
            <strong class="metric-value small">{{ modeLabel }}</strong>
            <span class="metric-icon">✦</span>
          </div>

          <div class="metric-card metric-card-wide floating-card slow">
            <span class="metric-title">Analysehinweis</span>
            <strong class="metric-value small">{{ status.message || "Warte auf Startsignal" }}</strong>
          </div>
        </div>
      </article>
    </section>
  </div>
</template>

<style scoped>
:global(body) {
  margin: 0;
}

.import-page {
  --bg-1: #f7fbff;
  --bg-2: #eef6ff;
  --bg-3: #ffffff;
  --text-1: #10233d;
  --text-2: #4f6482;
  --text-3: #7390b2;
  --border-1: rgba(126, 166, 217, 0.22);
  --glass-1: rgba(255, 255, 255, 0.68);
  --glass-2: rgba(255, 255, 255, 0.54);
  --shadow-1: 0 24px 80px rgba(86, 130, 190, 0.16);
  --shadow-2: 0 18px 44px rgba(65, 122, 198, 0.12);
  --accent-1: #67d5ff;
  --accent-2: #7f8cff;
  --accent-3: #9b6bff;
  --accent-4: #69f0d2;
  position: relative;
  min-height: 100vh;
  overflow: hidden;
  padding: 42px 24px 56px 24px;
  background:
      radial-gradient(circle at 10% 10%, rgba(103, 213, 255, 0.28), transparent 24%),
      radial-gradient(circle at 85% 14%, rgba(127, 140, 255, 0.24), transparent 24%),
      radial-gradient(circle at 50% 100%, rgba(105, 240, 210, 0.16), transparent 28%),
      linear-gradient(180deg, var(--bg-1) 0%, var(--bg-2) 46%, #edf4ff 100%);
  color: var(--text-1);
}

.import-page.running {
  background:
      radial-gradient(circle at 12% 10%, rgba(103, 213, 255, 0.34), transparent 24%),
      radial-gradient(circle at 85% 14%, rgba(127, 140, 255, 0.3), transparent 24%),
      radial-gradient(circle at 50% 100%, rgba(105, 240, 210, 0.2), transparent 28%),
      linear-gradient(180deg, #f9fcff 0%, #eef7ff 46%, #eaf4ff 100%);
}

.import-page.completed {
  background:
      radial-gradient(circle at 12% 10%, rgba(105, 240, 210, 0.3), transparent 22%),
      radial-gradient(circle at 85% 14%, rgba(127, 140, 255, 0.24), transparent 24%),
      radial-gradient(circle at 52% 100%, rgba(103, 213, 255, 0.2), transparent 28%),
      linear-gradient(180deg, #fbfffe 0%, #effffc 46%, #eefaff 100%);
}

.ambient {
  position: absolute;
  border-radius: 999px;
  filter: blur(70px);
  pointer-events: none;
  mix-blend-mode: screen;
  opacity: 0.8;
}

.ambient-1 {
  top: -120px;
  left: -120px;
  width: 380px;
  height: 380px;
  background: rgba(91, 215, 255, 0.5);
  animation: floatBlob 12s ease-in-out infinite;
}

.ambient-2 {
  top: 70px;
  right: -110px;
  width: 420px;
  height: 420px;
  background: rgba(135, 132, 255, 0.34);
  animation: floatBlobReverse 16s ease-in-out infinite;
}

.ambient-3 {
  bottom: -140px;
  left: 50%;
  transform: translateX(-50%);
  width: 520px;
  height: 300px;
  background: rgba(103, 255, 218, 0.24);
  animation: pulseGlow 9s ease-in-out infinite;
}

.noise-layer {
  position: absolute;
  inset: 0;
  opacity: 0.08;
  pointer-events: none;
  background-image:
      radial-gradient(circle at 20% 20%, rgba(255, 255, 255, 0.7) 0 0.5px, transparent 0.6px),
      radial-gradient(circle at 70% 30%, rgba(255, 255, 255, 0.7) 0 0.5px, transparent 0.6px),
      radial-gradient(circle at 35% 75%, rgba(255, 255, 255, 0.8) 0 0.5px, transparent 0.6px),
      radial-gradient(circle at 80% 80%, rgba(255, 255, 255, 0.7) 0 0.5px, transparent 0.6px);
  background-size: 180px 180px;
}

.grid-layer {
  position: absolute;
  inset: 0;
  pointer-events: none;
  opacity: 0.4;
  background-image:
      linear-gradient(rgba(125, 162, 211, 0.08) 1px, transparent 1px),
      linear-gradient(90deg, rgba(125, 162, 211, 0.08) 1px, transparent 1px);
  background-size: 42px 42px;
  mask-image: radial-gradient(circle at center, black 48%, transparent 100%);
}

.scan-line {
  position: absolute;
  inset: 0;
  pointer-events: none;
  overflow: hidden;
}

.scan-line::after {
  content: "";
  position: absolute;
  left: 0;
  right: 0;
  height: 140px;
  background: linear-gradient(180deg, transparent 0%, rgba(255, 255, 255, 0.08) 40%, rgba(103, 213, 255, 0.16) 50%, transparent 100%);
  animation: scanMove 7s linear infinite;
}

.particles-layer {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.particle {
  position: absolute;
  display: block;
  border-radius: 999px;
  background: radial-gradient(circle at 30% 30%, rgba(255, 255, 255, 0.95), rgba(110, 191, 255, 0.75) 45%, rgba(127, 140, 255, 0.25) 70%, transparent 100%);
  box-shadow:
      0 0 14px rgba(108, 193, 255, 0.8),
      0 0 28px rgba(127, 140, 255, 0.35);
  animation: floatParticle linear infinite;
}

.hero-card,
.panel {
  position: relative;
  z-index: 1;
  overflow: hidden;
  border: 1px solid var(--border-1);
  background:
      linear-gradient(180deg, rgba(255, 255, 255, 0.8) 0%, rgba(247, 251, 255, 0.62) 100%);
  box-shadow: var(--shadow-1);
  backdrop-filter: blur(24px);
}

.hero-card::before,
.panel::before {
  content: "";
  position: absolute;
  inset: 0;
  background:
      linear-gradient(130deg, rgba(255, 255, 255, 0.42), transparent 32%, transparent 68%, rgba(255, 255, 255, 0.24));
  pointer-events: none;
}

.hero-card {
  max-width: 1280px;
  margin: 0 auto 28px auto;
  border-radius: 34px;
  padding: 34px;
  animation: heroEntrance 0.9s ease;
}

.hero-topline {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 28px;
}

.hero-badge,
.hero-chip {
  position: relative;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  min-height: 42px;
  padding: 10px 16px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  border: 1px solid rgba(113, 176, 255, 0.24);
  background: rgba(255, 255, 255, 0.58);
  box-shadow: 0 10px 24px rgba(127, 170, 230, 0.12);
}

.hero-badge {
  color: #2d77c9;
}

.hero-chip {
  color: #6d67ff;
}

.badge-pulse,
.hero-chip-dot {
  width: 10px;
  height: 10px;
  border-radius: 999px;
  flex-shrink: 0;
}

.badge-pulse {
  background: linear-gradient(135deg, #50d9ff 0%, #6c9bff 100%);
  box-shadow: 0 0 0 0 rgba(93, 181, 255, 0.55);
  animation: badgePulse 2s infinite;
}

.hero-chip-dot {
  background: linear-gradient(135deg, #8d78ff 0%, #64c6ff 100%);
  box-shadow: 0 0 18px rgba(127, 140, 255, 0.55);
  animation: softBlink 1.8s infinite;
}

.hero-content {
  display: grid;
  grid-template-columns: minmax(0, 1.2fr) minmax(280px, 420px);
  gap: 24px;
  align-items: center;
}

.hero-title {
  margin: 0 0 14px 0;
  font-size: clamp(40px, 5vw, 74px);
  line-height: 0.95;
  font-weight: 900;
  letter-spacing: -0.05em;
  color: #132844;
}

.title-gradient {
  display: inline-block;
  background: linear-gradient(135deg, #43cbff 0%, #6e8cff 42%, #a06cff 100%);
  background-clip: text;
  -webkit-background-clip: text;
  color: transparent;
  filter: drop-shadow(0 10px 22px rgba(102, 167, 255, 0.16));
}

.hero-description {
  max-width: 760px;
  margin: 0;
  color: var(--text-2);
  font-size: 18px;
  line-height: 1.75;
}

.hero-actions {
  margin-top: 28px;
}

.start-button {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 0;
  border-radius: 22px;
  padding: 18px 30px;
  min-width: 240px;
  font-size: 16px;
  font-weight: 800;
  letter-spacing: 0.01em;
  color: #ffffff;
  cursor: pointer;
  background: transparent;
  box-shadow:
      0 18px 34px rgba(110, 149, 255, 0.22),
      0 8px 20px rgba(76, 211, 255, 0.2);
  transition:
      transform 0.22s ease,
      box-shadow 0.22s ease,
      opacity 0.22s ease,
      filter 0.22s ease;
}

.start-button:hover:not(:disabled) {
  transform: translateY(-3px) scale(1.01);
  box-shadow:
      0 24px 42px rgba(110, 149, 255, 0.26),
      0 12px 26px rgba(76, 211, 255, 0.24);
  filter: saturate(1.08);
}

.start-button:active:not(:disabled) {
  transform: translateY(-1px) scale(0.995);
}

.start-button:disabled {
  cursor: not-allowed;
  opacity: 0.74;
}

.button-background,
.button-ring,
.button-shine {
  position: absolute;
  inset: 0;
  border-radius: inherit;
}

.button-background {
  z-index: -3;
  background: linear-gradient(135deg, #55d8ff 0%, #6f95ff 48%, #a26bff 100%);
}

.button-ring {
  z-index: -2;
  inset: -2px;
  border-radius: 24px;
  background: linear-gradient(135deg, rgba(85, 216, 255, 0.4), rgba(162, 107, 255, 0.45));
  filter: blur(14px);
  opacity: 0.8;
  animation: buttonPulse 2.6s ease-in-out infinite;
}

.button-shine {
  z-index: -1;
  background:
      linear-gradient(115deg, transparent 15%, rgba(255, 255, 255, 0.3) 42%, rgba(255, 255, 255, 0.65) 50%, rgba(255, 255, 255, 0.28) 58%, transparent 82%);
  transform: translateX(-125%);
  animation: shine 2.8s linear infinite;
}

.button-text {
  position: relative;
  z-index: 1;
}

.hero-visual {
  display: flex;
  justify-content: center;
}

.radar-card {
  position: relative;
  width: min(100%, 360px);
  aspect-ratio: 1;
  border-radius: 32px;
  border: 1px solid rgba(122, 170, 230, 0.22);
  background:
      radial-gradient(circle at center, rgba(255, 255, 255, 0.78) 0%, rgba(242, 248, 255, 0.88) 52%, rgba(234, 243, 255, 0.96) 100%);
  box-shadow:
      inset 0 1px 0 rgba(255, 255, 255, 0.75),
      0 24px 54px rgba(92, 143, 209, 0.16);
  overflow: hidden;
  animation: floatCard 6s ease-in-out infinite;
}

.radar-card::before {
  content: "";
  position: absolute;
  inset: 20px;
  border-radius: 999px;
  background:
      radial-gradient(circle, rgba(103, 213, 255, 0.06) 0%, transparent 55%),
      repeating-radial-gradient(circle, rgba(111, 158, 255, 0.12) 0 1px, transparent 1px 34px);
  mask-image: radial-gradient(circle, black 72%, transparent 73%);
}

.radar-orbit {
  position: absolute;
  inset: 50%;
  border-radius: 999px;
  border: 1px solid rgba(110, 162, 232, 0.22);
  transform: translate(-50%, -50%);
}

.orbit-1 {
  width: 40%;
  height: 40%;
}

.orbit-2 {
  width: 62%;
  height: 62%;
}

.orbit-3 {
  width: 84%;
  height: 84%;
}

.radar-center {
  position: absolute;
  inset: 50%;
  width: 22px;
  height: 22px;
  border-radius: 999px;
  transform: translate(-50%, -50%);
  background: linear-gradient(135deg, #54d9ff 0%, #7f83ff 100%);
  box-shadow:
      0 0 20px rgba(84, 217, 255, 0.65),
      0 0 38px rgba(127, 131, 255, 0.45);
  animation: pingCenter 2s ease-in-out infinite;
}

.radar-sweep {
  position: absolute;
  inset: 50%;
  width: 84%;
  height: 84%;
  transform: translate(-50%, -50%);
  border-radius: 999px;
  background: conic-gradient(from 0deg, rgba(98, 221, 255, 0.32), transparent 22%, transparent 100%);
  filter: blur(1px);
  animation: rotateSweep 4s linear infinite;
  mask-image: radial-gradient(circle, transparent 0 12%, black 13% 100%);
}

.radar-stats {
  position: absolute;
  left: 20px;
  right: 20px;
  bottom: 20px;
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.radar-stat {
  padding: 14px 14px 12px 14px;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.68);
  border: 1px solid rgba(123, 173, 236, 0.18);
  box-shadow: 0 10px 18px rgba(90, 144, 209, 0.1);
  backdrop-filter: blur(12px);
}

.radar-stat span {
  display: block;
  margin-bottom: 6px;
  color: #6c84a5;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.radar-stat strong {
  display: block;
  color: #173153;
  font-size: 19px;
  font-weight: 850;
}

.dashboard-grid {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1.35fr 1fr;
  gap: 28px;
  max-width: 1280px;
  margin: 0 auto;
}

.panel {
  border-radius: 32px;
  padding: 28px;
  box-shadow: var(--shadow-2);
  animation: panelUp 0.8s ease;
}

.panel-glow {
  position: absolute;
  top: -80px;
  right: -40px;
  width: 220px;
  height: 220px;
  border-radius: 999px;
  background: radial-gradient(circle, rgba(103, 213, 255, 0.3) 0%, transparent 70%);
  filter: blur(20px);
  pointer-events: none;
}

.panel-glow-secondary {
  background: radial-gradient(circle, rgba(127, 140, 255, 0.24) 0%, transparent 70%);
}

.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.panel-label {
  color: #5e95d6;
  font-size: 12px;
  font-weight: 800;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

.panel-title {
  margin-top: 8px;
  font-size: 30px;
  font-weight: 900;
  letter-spacing: -0.03em;
  color: #173153;
}

.status-indicator {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  min-height: 44px;
  padding: 10px 16px;
  border-radius: 999px;
  border: 1px solid rgba(128, 165, 217, 0.16);
  background: rgba(255, 255, 255, 0.72);
  color: #6d7f98;
  font-size: 14px;
  font-weight: 700;
  box-shadow: 0 10px 20px rgba(108, 149, 204, 0.08);
}

.status-indicator.active {
  color: #158c6a;
  background: linear-gradient(180deg, rgba(227, 255, 246, 0.95) 0%, rgba(215, 255, 240, 0.82) 100%);
  border-color: rgba(74, 220, 175, 0.25);
}

.status-dot {
  width: 10px;
  height: 10px;
  border-radius: 999px;
  background: currentColor;
  box-shadow: 0 0 16px currentColor;
  animation: softBlink 1.8s infinite;
}

.status-message {
  margin: 22px 0 28px 0;
  color: var(--text-2);
  font-size: 16px;
  line-height: 1.75;
}

.progress-section {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.progress-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #5f7798;
  font-size: 15px;
  font-weight: 600;
}

.progress-meta strong {
  font-size: 16px;
  color: #193153;
}

.progress-track {
  position: relative;
  overflow: hidden;
  height: 22px;
  border-radius: 999px;
  background:
      linear-gradient(180deg, rgba(255, 255, 255, 0.9) 0%, rgba(236, 244, 255, 0.95) 100%);
  border: 1px solid rgba(128, 171, 229, 0.16);
  box-shadow:
      inset 0 2px 10px rgba(153, 180, 219, 0.12),
      0 10px 22px rgba(111, 149, 204, 0.08);
}

.progress-track-glow {
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, rgba(85, 216, 255, 0.08), rgba(127, 140, 255, 0.06), rgba(162, 107, 255, 0.08));
}

.progress-fill {
  position: relative;
  height: 100%;
  border-radius: inherit;
  background:
      linear-gradient(90deg, #52d8ff 0%, #6f95ff 48%, #9c6bff 100%);
  transition: width 0.55s cubic-bezier(0.22, 1, 0.36, 1);
  box-shadow:
      0 0 22px rgba(96, 181, 255, 0.42),
      0 0 40px rgba(127, 140, 255, 0.24);
  overflow: hidden;
}

.progress-fill::before {
  content: "";
  position: absolute;
  inset: 0;
  background:
      linear-gradient(115deg, transparent 15%, rgba(255, 255, 255, 0.18) 38%, rgba(255, 255, 255, 0.52) 50%, rgba(255, 255, 255, 0.16) 62%, transparent 85%);
  animation: shine 2.2s linear infinite;
}

.progress-fill::after {
  content: "";
  position: absolute;
  inset: 0;
  background:
      repeating-linear-gradient(
          135deg,
          rgba(255, 255, 255, 0.18) 0 10px,
          rgba(255, 255, 255, 0.06) 10px 20px
      );
  mix-blend-mode: screen;
  opacity: 0.6;
}

.progress-spark {
  position: absolute;
  top: 50%;
  width: 16px;
  height: 16px;
  border-radius: 999px;
  transform: translateY(-50%);
  background: rgba(255, 255, 255, 0.95);
  filter: blur(1px);
  box-shadow:
      0 0 12px rgba(255, 255, 255, 0.95),
      0 0 22px rgba(255, 255, 255, 0.65);
}

.progress-spark-1 {
  right: 16px;
  animation: driftSpark 1.8s ease-in-out infinite;
}

.progress-spark-2 {
  right: 42px;
  width: 10px;
  height: 10px;
  opacity: 0.8;
  animation: driftSpark 1.4s ease-in-out infinite reverse;
}

.progress-spark-3 {
  right: 68px;
  width: 8px;
  height: 8px;
  opacity: 0.6;
  animation: driftSpark 1.2s ease-in-out infinite;
}

.timeline-row {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-top: 22px;
}

.timeline-chip {
  padding: 10px 14px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 800;
  letter-spacing: 0.04em;
  color: #6982a1;
  border: 1px solid rgba(126, 167, 220, 0.14);
  background: rgba(255, 255, 255, 0.66);
  transition:
      transform 0.24s ease,
      background 0.24s ease,
      color 0.24s ease,
      box-shadow 0.24s ease;
}

.timeline-chip.active {
  color: #ffffff;
  background: linear-gradient(135deg, #58d8ff 0%, #738fff 50%, #9a6dff 100%);
  box-shadow: 0 12px 24px rgba(110, 149, 255, 0.22);
  transform: translateY(-1px);
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-top: 18px;
}

.metric-card {
  position: relative;
  overflow: hidden;
  min-height: 144px;
  padding: 20px;
  border-radius: 24px;
  background:
      linear-gradient(180deg, rgba(255, 255, 255, 0.78) 0%, rgba(244, 249, 255, 0.72) 100%);
  border: 1px solid rgba(125, 170, 229, 0.16);
  box-shadow:
      inset 0 1px 0 rgba(255, 255, 255, 0.72),
      0 14px 26px rgba(100, 139, 199, 0.1);
}

.metric-card::before {
  content: "";
  position: absolute;
  inset: 0;
  background:
      linear-gradient(135deg, rgba(85, 216, 255, 0.08), transparent 38%, transparent 62%, rgba(162, 107, 255, 0.1));
  pointer-events: none;
}

.metric-card-wide {
  grid-column: 1 / -1;
  min-height: 156px;
}

.floating-card {
  animation: floatMetric 6s ease-in-out infinite;
}

.floating-card.delayed {
  animation-delay: 0.8s;
}

.floating-card.slow {
  animation-delay: 1.4s;
  animation-duration: 7.5s;
}

.metric-title {
  display: block;
  margin-bottom: 14px;
  color: #6b86a8;
  font-size: 12px;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.metric-value {
  position: relative;
  z-index: 1;
  display: block;
  font-size: 38px;
  font-weight: 900;
  letter-spacing: -0.04em;
  color: #162f4f;
}

.metric-value.small {
  font-size: 20px;
  line-height: 1.55;
  letter-spacing: -0.02em;
}

.metric-backdrop-number {
  position: absolute;
  right: 14px;
  bottom: 4px;
  font-size: 84px;
  line-height: 1;
  font-weight: 900;
  letter-spacing: -0.07em;
  color: rgba(96, 141, 212, 0.08);
  user-select: none;
  pointer-events: none;
}

.metric-icon {
  position: absolute;
  right: 20px;
  bottom: 16px;
  font-size: 54px;
  line-height: 1;
  color: rgba(123, 132, 255, 0.14);
  transform: rotate(10deg);
}

@keyframes heroEntrance {
  0% {
    opacity: 0;
    transform: translateY(18px) scale(0.985);
  }

  100% {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes panelUp {
  0% {
    opacity: 0;
    transform: translateY(22px);
  }

  100% {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes shine {
  0% {
    transform: translateX(-125%);
  }

  100% {
    transform: translateX(125%);
  }
}

@keyframes scanMove {
  0% {
    transform: translateY(-180px);
  }

  100% {
    transform: translateY(calc(100vh + 180px));
  }
}

@keyframes rotateSweep {
  0% {
    transform: translate(-50%, -50%) rotate(0deg);
  }

  100% {
    transform: translate(-50%, -50%) rotate(360deg);
  }
}

@keyframes pingCenter {
  0%,
  100% {
    transform: translate(-50%, -50%) scale(1);
    box-shadow:
        0 0 20px rgba(84, 217, 255, 0.65),
        0 0 38px rgba(127, 131, 255, 0.45);
  }

  50% {
    transform: translate(-50%, -50%) scale(1.16);
    box-shadow:
        0 0 28px rgba(84, 217, 255, 0.82),
        0 0 54px rgba(127, 131, 255, 0.58);
  }
}

@keyframes floatCard {
  0%,
  100% {
    transform: translateY(0) rotate(0deg);
  }

  50% {
    transform: translateY(-8px) rotate(0.6deg);
  }
}

@keyframes floatMetric {
  0%,
  100% {
    transform: translateY(0);
  }

  50% {
    transform: translateY(-6px);
  }
}

@keyframes floatBlob {
  0%,
  100% {
    transform: translate3d(0, 0, 0) scale(1);
  }

  50% {
    transform: translate3d(26px, 18px, 0) scale(1.08);
  }
}

@keyframes floatBlobReverse {
  0%,
  100% {
    transform: translate3d(0, 0, 0) scale(1);
  }

  50% {
    transform: translate3d(-24px, 28px, 0) scale(1.06);
  }
}

@keyframes pulseGlow {
  0%,
  100% {
    opacity: 0.7;
    transform: translateX(-50%) scale(1);
  }

  50% {
    opacity: 1;
    transform: translateX(-50%) scale(1.08);
  }
}

@keyframes badgePulse {
  0% {
    box-shadow: 0 0 0 0 rgba(93, 181, 255, 0.52);
  }

  70% {
    box-shadow: 0 0 0 12px rgba(93, 181, 255, 0);
  }

  100% {
    box-shadow: 0 0 0 0 rgba(93, 181, 255, 0);
  }
}

@keyframes softBlink {
  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.55;
  }
}

@keyframes buttonPulse {
  0%,
  100% {
    transform: scale(1);
    opacity: 0.78;
  }

  50% {
    transform: scale(1.03);
    opacity: 1;
  }
}

@keyframes driftSpark {
  0%,
  100% {
    transform: translateY(-50%) translateX(0) scale(1);
  }

  50% {
    transform: translateY(-50%) translateX(6px) scale(1.14);
  }
}

@keyframes floatParticle {
  0% {
    transform: translate3d(0, 0, 0) scale(0.85);
    opacity: 0;
  }

  12% {
    opacity: 1;
  }

  50% {
    transform: translate3d(18px, -24px, 0) scale(1.12);
  }

  100% {
    transform: translate3d(-12px, -54px, 0) scale(0.92);
    opacity: 0;
  }
}

@media (max-width: 1024px) {
  .hero-content,
  .dashboard-grid {
    grid-template-columns: 1fr;
  }

  .hero-visual {
    justify-content: flex-start;
  }
}

@media (max-width: 768px) {
  .import-page {
    padding: 18px 16px 34px 16px;
  }

  .hero-card,
  .panel {
    padding: 22px;
    border-radius: 26px;
  }

  .hero-topline,
  .panel-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .hero-title {
    font-size: 40px;
  }

  .radar-card {
    width: 100%;
    max-width: 340px;
  }
}

@media (max-width: 600px) {
  .metrics-grid {
    grid-template-columns: 1fr;
  }

  .metric-card-wide {
    grid-column: auto;
  }

  .start-button {
    width: 100%;
  }

  .timeline-row {
    gap: 10px;
  }

  .timeline-chip {
    width: 100%;
    text-align: center;
  }

  .hero-title {
    font-size: 34px;
  }

  .hero-description {
    font-size: 16px;
  }
}
</style>