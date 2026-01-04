document.addEventListener('DOMContentLoaded', function () {
    console.log("CSharpClicker: Script loaded");

    function formatCompactNumber(number) {
        if (number < 1000) return Math.floor(number);
        if (number < 1000000) return formatValue(number, 1000, "K");
        if (number < 1000000000) return formatValue(number, 1000000, "M");
        if (number < 1000000000000) return formatValue(number, 1000000000, "B");
        return formatValue(number, 1000000000000, "T");
    }

    function formatValue(number, divisor, suffix) {
        let value = number / divisor;
        let shortValue = parseFloat(value.toFixed(2));
        if (shortValue >= 100) {
            shortValue = Math.round(shortValue);
        }
        return shortValue + suffix;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/clickerHub')
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(function () {
            console.log('SignalR: Connected');
        })
        .catch(function (err) {
            return console.error('SignalR Error:', err.toString());
        });

    // Обновление общего счета
    connection.on('ScoreUpdated', function (current, record) {
        const currentScoreElement = document.getElementById('currentScore');
        const recordScoreElement = document.getElementById('recordScore');

        if (currentScoreElement) {
            currentScoreElement.setAttribute('data-val', current);
            currentScoreElement.textContent = formatCompactNumber(current);
        }

        if (recordScoreElement) {
            recordScoreElement.setAttribute('data-val', record);
            recordScoreElement.textContent = formatCompactNumber(record);
        }

        updateBoostsAvailability();
    });

    // Обновление прибыли
    connection.on('ProfitUpdated', function (profitPerClick, profitPerSecond) {
        const profitPerClickElement = document.getElementById('profitPerClick');
        const profitPerSecondElement = document.getElementById('profitPerSecond');

        if (profitPerClickElement) profitPerClickElement.textContent = formatCompactNumber(profitPerClick);
        if (profitPerSecondElement) profitPerSecondElement.textContent = formatCompactNumber(profitPerSecond);
    });

    // Обновление магазина
    connection.on('BoostUpdated', function (boostId, quantity, currentPrice, nextProfit) {
        const boostElement = document.querySelector(`[data-boost-id="${boostId}"]`);

        if (boostElement) {
            const priceElement = boostElement.querySelector('[data-boost-price]');
            const quantityElement = boostElement.querySelector('[data-boost-quantity]');
            const profitElement = boostElement.querySelector('[data-boost-profit]');

            if (priceElement) {
                priceElement.setAttribute('data-boost-price', currentPrice);
                priceElement.textContent = formatCompactNumber(currentPrice);
            }
            if (quantityElement) quantityElement.textContent = quantity;
            if (profitElement) profitElement.textContent = formatCompactNumber(nextProfit);

            boostElement.style.borderColor = "#ffc107";
            setTimeout(() => boostElement.style.borderColor = "transparent", 300);

            updateBoostsAvailability();
        }
    });

    const clickButton = document.getElementById('click-item');
    if (clickButton) {
        clickButton.addEventListener('click', async function (e) {

            spawnFloatingText(e);

            try {
                await connection.invoke('RegisterClicks', 1);
            } catch (err) {
                console.error("Click Error:", err);
            }
        });
    }

    function spawnFloatingText(e) {
        const profitElement = document.getElementById('profitPerClick');
        const profitText = profitElement ? profitElement.innerText : "+1";

        const el = document.createElement('div');
        el.classList.add('floating-text');
        el.innerText = "+" + profitText;

        el.style.position = 'fixed';
        el.style.pointerEvents = 'none'; 

        const randomX = Math.floor(Math.random() * 50) - 25;
        const randomY = Math.floor(Math.random() * 20) - 10;

        el.style.left = (e.clientX + randomX) + 'px';
        el.style.top = (e.clientY + randomY - 40) + 'px'; 

        document.body.appendChild(el);

        setTimeout(() => {
            el.remove();
        }, 1000);
    }

    // Покупки
    const boostCards = document.querySelectorAll('.boost-card');
    boostCards.forEach(function (card) {
        const boostId = card.getAttribute('data-boost-id');
        const buyButton = card.querySelector('.buy-boost-button');

        if (buyButton) {
            buyButton.addEventListener('click', async function (e) {
                e.preventDefault();

                if (buyButton.disabled) return;

                try {
                    await connection.invoke('BuyBoost', parseInt(boostId, 10));
                } catch (err) {
                    console.error("Buy Error:", err);
                }
            });
        }
    });

    updateBoostsAvailability();

    function updateBoostsAvailability() {
        const currentScoreElement = document.getElementById('currentScore');
        if (!currentScoreElement) return;

        const rawScore = currentScoreElement.getAttribute('data-val');
        const currentScore = parseInt(rawScore, 10) || 0;

        const cards = document.querySelectorAll('.boost-card');
        cards.forEach(function (card) {
            const priceElement = card.querySelector('[data-boost-price]');
            const buyButton = card.querySelector('.buy-boost-button');

            if (priceElement && buyButton) {
                const rawPrice = priceElement.getAttribute('data-boost-price');
                const price = parseInt(rawPrice, 10);

                if (currentScore < price) {
                    card.classList.add('disabled');
                    buyButton.disabled = true;
                    buyButton.style.cursor = "not-allowed";
                    buyButton.style.opacity = "0.5";
                } else {
                    card.classList.remove('disabled');
                    buyButton.disabled = false;
                    buyButton.style.cursor = "pointer";
                    buyButton.style.opacity = "1";
                }
            }
        });
    }
});