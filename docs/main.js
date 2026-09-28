// Hero demo: without Caffeine the mock screen counts down and "sleeps";
// clicking the tray cup keeps it awake, like the real app.
const demo = document.getElementById('demo');
const cup = demo.querySelector('.js-cup');
const statusEl = demo.querySelector('.js-status');
const tipEl = demo.querySelector('.js-tip');
const taskEl = demo.querySelector('.js-task');
const barEl = demo.querySelector('.js-bar');

const SLEEP_AFTER = 9;
let on = false;
let asleep = false;
let countdown = SLEEP_AFTER;
let progress = 12;

function render() {
  demo.classList.toggle('is-on', on);
  demo.classList.toggle('is-asleep', asleep);
  cup.setAttribute('aria-pressed', String(on));
  statusEl.textContent = on ? demo.dataset.on : demo.dataset.off.replace('{s}', countdown);
  barEl.style.width = progress + '%';
  taskEl.textContent = progress >= 100 ? demo.dataset.done : demo.dataset.task.replace('{p}', progress);
}

function showTip() {
  tipEl.textContent = on ? demo.dataset.tipOn : demo.dataset.tipOff;
  clearTimeout(showTip.timer);
  showTip.timer = setTimeout(() => { tipEl.textContent = ''; }, 2600);
}

cup.addEventListener('click', () => {
  demo.classList.add('touched');
  on = !on;
  asleep = false;
  countdown = SLEEP_AFTER;
  showTip();
  render();
});

setInterval(() => {
  if (asleep) return;
  if (progress < 100) progress = Math.min(100, progress + 2);
  if (!on) {
    countdown -= 1;
    if (countdown <= 0) { countdown = 0; asleep = true; }
  }
  render();
}, 1000);

render();
