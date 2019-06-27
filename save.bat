@echo off
git archive --format=zip HEAD --output=../coolc[last_commit].zip
7z a ..\coolc[current_state].zip ..\coolc\
