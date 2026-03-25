/* ============================================================
   PhotoApp — infinite-scroll.js
   Підключається на Index, Profile, MyPosts
   ============================================================ */

window.InfiniteScroll = (function () {

  // ── Рендер картки поста (дзеркалить _PostCard.cshtml) ─────
  function renderPostCard(post) {
    const isReplyClass = post.isReply ? 'post-card--reply' : '';
    const replyBadge = post.isReply ? `
      <div class="post-card__badge post-card__badge--reply">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
          <polyline points="9 14 4 9 9 4"/><path d="M20 20v-7a4 4 0 0 0-4-4H4"/>
        </svg>
        Відповідь на пост
        <a href="/Image/Profile/${post.originalPostUserId}">${post.originalPostUserName}</a>
      </div>` : '';

    const originalPreview = post.isReply ? `
      <a href="/Image/Profile/${post.originalPostUserId}" class="post-card__original-preview">
        <img src="${post.originalPostImagePath}" class="post-card__original-thumb" />
        <div class="post-card__original-info">
          <span class="post-card__original-author">${post.originalPostUserName}</span>
          <span class="post-card__original-desc">${escHtml(post.originalPostDescription || '')}</span>
        </div>
      </a>` : '';

    const likeBtn = post.isLikedByMe
      ? `<form method="post" action="/Image/Unlike/${post.id}">
           <button class="post-card__btn post-card__btn--active">
             <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" stroke-width="2">
               <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
             </svg> Лайк
           </button>
         </form>`
      : `<form method="post" action="/Image/Like/${post.id}">
           <button class="post-card__btn">
             <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
               <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
             </svg> Лайк
           </button>
         </form>`;

    const forwardBtn = !post.isOwnPost
      ? `<a href="/PostInteraction/Forward/${post.id}" class="post-card__btn">
           <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
             <polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/>
             <polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/>
           </svg> Переслати
         </a>` : '';

    const cardId = `post-${post.id}`;

    return `
      <div class="post-card ${isReplyClass} post-card--animate">
        ${replyBadge}
        <a href="/Image/Profile/${post.userId}" class="post-card__img-wrap">
          <img src="${post.imagePath}" class="post-card__img" alt="${escHtml(post.description || '')}" loading="lazy" />
        </a>
        <div class="post-card__body">
          ${originalPreview}
          ${post.description ? `<p class="post-card__desc">${escHtml(post.description)}</p>` : ''}
          <div class="post-card__stats">
            <span class="post-card__stat" title="Лайки">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="${post.isLikedByMe ? 'currentColor' : 'none'}" stroke="currentColor" stroke-width="2">
                <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
              </svg> ${post.likesCount}
            </span>
            <span class="post-card__stat" title="Коментарі">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
              </svg> ${post.commentsCount}
            </span>
            <span class="post-card__stat" title="Відповіді">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="9 14 4 9 9 4"/><path d="M20 20v-7a4 4 0 0 0-4-4H4"/>
              </svg> ${post.repliesCount}
            </span>
            <span class="post-card__stat" title="Пересилань">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/>
                <polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/>
              </svg> ${post.forwardsCount}
            </span>
            <span class="post-card__date">${post.createdAt}</span>
          </div>
          <div class="post-card__actions">
            ${likeBtn}
            <a href="/PostInteraction/Reply/${post.id}" class="post-card__btn">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="9 14 4 9 9 4"/><path d="M20 20v-7a4 4 0 0 0-4-4H4"/>
              </svg> Відповісти
            </a>
            ${forwardBtn}
            <button class="post-card__btn post-card__btn--ghost"
                    onclick="toggleComments('${cardId}')" type="button">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
              </svg> Коментарі
            </button>
          </div>
          <div class="post-card__comments" id="${cardId}" style="display:none;">
            <form method="post" action="/Comment/Add" class="post-card__comment-form">
              <input type="hidden" name="imagePostId" value="${post.id}" />
              <textarea name="content" class="post-card__comment-input"
                        rows="2" maxlength="1000"
                        placeholder="Напишіть коментар..." required></textarea>
              <button type="submit" class="post-card__comment-submit">Надіслати</button>
            </form>
            <p class="post-card__no-comments">Завантажте сторінку щоб побачити коментарі.</p>
          </div>
        </div>
      </div>`;
  }

  // ── HTML escape ────────────────────────────────────────────
  function escHtml(str) {
    return String(str)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }

  // ── Ініціалізація ──────────────────────────────────────────
  function init(options) {
    const {
      apiUrl,          // URL ендпоінту
      gridSelector,    // CSS селектор .posts-grid
      pageSize = 12
    } = options;

    const grid = document.querySelector(gridSelector);
    if (!grid) return;

    let currentPage = 1;
    let isLoading = false;
    let hasMore = true;

    // ── Спіннер ──────────────────────────────────────────────
    const spinner = document.createElement('div');
    spinner.className = 'inf-spinner';
    spinner.innerHTML = `
      <div class="inf-spinner__ring"></div>
      <span>Завантаження...</span>`;
    spinner.style.display = 'none';
    grid.parentNode.insertBefore(spinner, grid.nextSibling);

    // ── "Більше немає" ───────────────────────────────────────
    const endMsg = document.createElement('div');
    endMsg.className = 'inf-end';
    endMsg.textContent = '✓ Більше постів немає';
    endMsg.style.display = 'none';
    spinner.parentNode.insertBefore(endMsg, spinner.nextSibling);

    // ── "Повторити" при помилці ──────────────────────────────
    const errMsg = document.createElement('div');
    errMsg.className = 'inf-error';
    errMsg.style.display = 'none';
    errMsg.innerHTML = `
      <span>Помилка завантаження</span>
      <button class="inf-error__retry">Повторити</button>`;
    errMsg.querySelector('.inf-error__retry').addEventListener('click', () => {
      errMsg.style.display = 'none';
      loadPage();
    });
    endMsg.parentNode.insertBefore(errMsg, endMsg.nextSibling);

    // ── Завантаження сторінки ────────────────────────────────
    async function loadPage() {
      if (isLoading || !hasMore) return;
      isLoading = true;
      spinner.style.display = 'flex';
      errMsg.style.display = 'none';

      try {
        const sep = apiUrl.includes('?') ? '&' : '?';
        const res = await fetch(`${apiUrl}${sep}page=${currentPage}&pageSize=${pageSize}`);

        if (!res.ok) throw new Error(`HTTP ${res.status}`);

        const data = await res.json();

        // Додаємо картки
        data.items.forEach(post => {
          const div = document.createElement('div');
          div.innerHTML = renderPostCard(post);
          const card = div.firstElementChild;
          grid.appendChild(card);
          // Запускаємо анімацію після вставки
          requestAnimationFrame(() => card.classList.add('post-card--visible'));
        });

        hasMore = data.hasMore;
        currentPage++;

        if (!hasMore) {
          endMsg.style.display = 'block';
        } else {
          // Якщо після завантаження сторінка не потребує скролу — одразу підвантажуємо ще
          setTimeout(() => {
            const scrolled = window.scrollY + window.innerHeight;
            const threshold = document.documentElement.scrollHeight * 0.85;
            if (scrolled >= threshold) loadPage();
          }, 150);
        }

      } catch (e) {
        console.error('InfiniteScroll error:', e);
        errMsg.style.display = 'flex';
      } finally {
        isLoading = false;
        spinner.style.display = 'none';
      }
    }

    // ── Scroll listener з throttle ───────────────────────────
    let ticking = false;
    window.addEventListener('scroll', () => {
      if (ticking) return;
      ticking = true;
      requestAnimationFrame(() => {
        const scrolled = window.scrollY + window.innerHeight;
        const threshold = document.documentElement.scrollHeight * 0.85;
        if (scrolled >= threshold) loadPage();
        ticking = false;
      });
    }, { passive: true });

    // Завантажуємо першу порцію
    loadPage();
  }

  return { init };
})();
