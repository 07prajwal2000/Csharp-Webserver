const btn = document.getElementById("ctr-btn");
const counterTag = document.getElementById("ctr");
let counter = 0;
btn.onclick = () => {
  counterTag.innerText = ++counter;
};