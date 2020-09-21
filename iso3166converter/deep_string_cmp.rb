# frozen_string_literal: true

module Utils
    def deep_string_cmp(a, b)
        a = a[0..(b.length - 1)]
        b = b[0..(a.length - 1)]
        i = 0
        a.chars.zip(b.chars).each do |x|
            i += 1 if x[0] == x[1]
        end
        i
    end

    module_function :deep_string_cmp
end
